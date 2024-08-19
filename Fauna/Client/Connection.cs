using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Fauna.Mapping;
using Polly;

namespace Fauna;

/// <summary>
/// A class that handles HTTP requests and retries.
/// </summary>
internal class Connection : IConnection
{
    private readonly Configuration _cfg;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the Connection class.
    /// </summary>
    /// <param name="configuration">The <see cref="Configuration"/> to use.</param>
    public Connection(Configuration configuration)
    {
        _cfg = configuration;
    }

    public async Task<HttpResponseMessage> DoPostAsync(
        string path,
        Stream body,
        Dictionary<string, string> headers,
        CancellationToken cancel = default)
    {
        HttpResponseMessage response;
        {
            var policyResult = await _cfg.RetryConfiguration.RetryPolicy
                .ExecuteAndCaptureAsync(async () =>
                    await _cfg.HttpClient.SendAsync(CreateHttpRequest(path, body, headers), cancel))
                .ConfigureAwait(false);
            if (policyResult.Outcome == OutcomeType.Successful)
            {
                response = policyResult.Result;
            }
            else
            {
                throw policyResult.FinalException;
            }
        }

        return response;
    }

    public async IAsyncEnumerable<Event<T>> OpenStream<T>(
        string path,
        Types.Stream stream,
        Dictionary<string, string> headers,
        MappingContext ctx,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : notnull
    {
        cancellationToken.ThrowIfCancellationRequested();

        while (!cancellationToken.IsCancellationRequested)
        {
            var listener = new EventListener<T>();
            Task streamTask = _cfg.RetryConfiguration.RetryPolicy.ExecuteAndCaptureAsync(async () =>
            {
                var streamData = new MemoryStream();
                stream.Serialize(streamData);

                var response = await _cfg.HttpClient
                    .SendAsync(
                        CreateHttpRequest(path, streamData, headers),
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken)
                    .ConfigureAwait(false);

                await using var streamAsync = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var streamReader = new StreamReader(streamAsync);

                while (!streamReader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {
                    string? line = await streamReader.ReadLineAsync().WaitAsync(cancellationToken);
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    var evt = Event<T>.From(line, ctx);
                    stream.StartTs = evt.TxnTime;
                    listener.Dispatch(evt);
                }

                listener.Close();
                return response;
            });

            await foreach (var evt in listener.Events().WithCancellation(cancellationToken))
            {
                yield return evt;
            }

            await streamTask;
        }
    }

    /// <summary>
    /// A helper class for handling events in a thread-safe manner.
    /// </summary>
    /// <typeparam name="T">The type of event data.</typeparam>
    private class EventListener<T> where T : notnull
    {
        private readonly ConcurrentQueue<Event<T>> _queue = new();
        private readonly SemaphoreSlim _semaphore = new(0);
        private bool _closed;

        public void Dispatch(Event<T> evt)
        {
            _queue.Enqueue(evt);
            _semaphore.Release();
        }

        public async IAsyncEnumerable<Event<T>> Events()
        {
            while (true)
            {
                await _semaphore.WaitAsync();

                if (_closed)
                {
                    _semaphore.Release();
                    yield break;
                }

                if (_queue.TryDequeue(out var evt))
                {
                    yield return evt;
                }

                _semaphore.Release();
            }
        }

        public void Close()
        {
            _closed = true;
        }
    }

    private HttpRequestMessage CreateHttpRequest(string path, Stream body, Dictionary<string, string> headers)
    {
        body.Position = 0;
        var request = new HttpRequestMessage
        {
            Content = new StreamContent(body),
            Method = HttpMethod.Post,
            RequestUri = new Uri(_cfg.Endpoint, path)
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

        foreach (var header in headers)
        {
            request.Headers.Add(header.Key, header.Value);
        }

        return request;
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing && _cfg.DisposeHttpClient)
        {
            _cfg.HttpClient.Dispose();
            GC.SuppressFinalize(this);
        }

        _disposed = true;
    }

    /// <summary>
    /// Disposes the resources used by the <see cref="Connection"/> class.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    // A finalizer: https://stackoverflow.com/questions/151051/when-should-i-use-gc-suppressfinalize
    ~Connection()
    {
        Dispose(false);
    }
}
