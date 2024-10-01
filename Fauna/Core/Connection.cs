﻿using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;
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
        {
            var policyResult = await _cfg.RetryConfiguration.RetryPolicy
                .ExecuteAndCaptureAsync(async () =>
                    await _cfg.HttpClient.SendAsync(CreateHttpRequest(path, body, headers), cancel))
                .ConfigureAwait(false);
            response = policyResult.Outcome == OutcomeType.Successful
                ? policyResult.Result
                : policyResult.FinalHandledResult ?? throw policyResult.FinalException;
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
            using var bc = new BlockingCollection<Event<T>>(new ConcurrentQueue<Event<T>>());
            Task<PolicyResult<HttpResponseMessage>> streamTask =
                _cfg.RetryConfiguration.RetryPolicy.ExecuteAndCaptureAsync(async () =>
                {
                    var streamData = new MemoryStream();
                    stream.Serialize(streamData);

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
                        stream.LastCursor = evt.Cursor;
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
