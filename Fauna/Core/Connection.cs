using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;
using Fauna.Util;
using Microsoft.Extensions.Logging;
using Polly;
using Stream = System.IO.Stream;

namespace Fauna.Core;

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

        var policyResult = await _cfg.RetryConfiguration.RetryPolicy
            .ExecuteAndCaptureAsync(() =>
                _cfg.HttpClient.SendAsync(CreateHttpRequest(path, body, headers), cancel))
            .ConfigureAwait(false);
        response = policyResult.Outcome == OutcomeType.Successful
            ? policyResult.Result
            : policyResult.FinalHandledResult ?? throw policyResult.FinalException;

        Logger.Instance.LogDebug(
            "Fauna HTTP Response {status} from {uri}, headers: {headers}",
            response.StatusCode.ToString(),
            response.RequestMessage?.RequestUri?.ToString() ?? "UNKNOWN",
            JsonSerializer.Serialize(
                response.Headers.ToDictionary(kv => kv.Key, kv => kv.Value.ToList()))
        );

        Logger.Instance.LogTrace("Response body: {body}", await response.Content.ReadAsStringAsync(cancel));

        return response;
    }

    public async IAsyncEnumerable<Event<T>> OpenStream<T>(
        string path,
        Types.EventSource eventSource,
        Dictionary<string, string> headers,
        MappingContext ctx,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : notnull
    {
        cancellationToken.ThrowIfCancellationRequested();

        while (!cancellationToken.IsCancellationRequested)
        {
            using var bc = new BlockingCollection<Event<T>>(new ConcurrentQueue<Event<T>>());
            Task<PolicyResult<HttpResponseMessage>> streamTask =
                _cfg.RetryConfiguration.RetryPolicy.ExecuteAndCaptureAsync(async () =>
                {
                    var streamData = new MemoryStream();
                    eventSource.Serialize(streamData);

                    var response = await _cfg.HttpClient
                        .SendAsync(
                            CreateHttpRequest(path, streamData, headers),
                            HttpCompletionOption.ResponseHeadersRead,
                            cancellationToken)
                        .ConfigureAwait(false);

                    if (!response.IsSuccessStatusCode)
                    {
                        bc.CompleteAdding();
                        return response;
                    }

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
                        eventSource.LastCursor = evt.Cursor;
                        bc.Add(evt, cancellationToken);
                    }

                    return response;
                });

            foreach (var evt in bc.GetConsumingEnumerable(cancellationToken))
            {
                yield return evt;
            }

            await streamTask;
            bc.CompleteAdding();
            if (streamTask.Result.Result.IsSuccessStatusCode)
            {
                continue;
            }

            var httpResponse = streamTask.Result.Result;
            string body = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

            throw ExceptionHandler.FromRawResponse(body, httpResponse);
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

        Logger.Instance.LogDebug(
            "Fauna HTTP {method} Request to {uri} (timeout {timeout}ms), headers: {headers}",
            HttpMethod.Post.ToString(),
            request.RequestUri.ToString(),
            _cfg.HttpClient.Timeout.TotalMilliseconds,
            JsonSerializer.Serialize(
                request.Headers
                    .Select(header =>
                    {
                        // Redact Auth header in debug logs
                        if (header.Key.StartsWith("Authorization", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return KeyValuePair.Create(header.Key, new[] { "hidden" }.AsEnumerable());
                        }

                        return header;
                    })
                    .ToDictionary(kv => kv.Key, kv => kv.Value.ToList()))
        );

        // Emit unredacted Auth header and response body in trace logs
        Logger.Instance.LogTrace("Unredacted Authorization header: {value}", request.Headers.Authorization?.ToString() ?? "null");
        Logger.Instance.LogTrace("Request body: {body}", request.Content.ReadAsStringAsync().Result);

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
