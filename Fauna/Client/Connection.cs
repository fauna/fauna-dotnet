using System.Net.Http.Headers;
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

    public async Task<StreamReader> OpenStream(
        string path,
        Stream body,
        Dictionary<string, string> headers,
        CancellationToken cancellationToken = default)
    {
        var policyResult = await _cfg.RetryConfiguration.StreamRetryPolicy
            .ExecuteAndCaptureAsync(async () =>
            {
                var response = await _cfg.HttpClient
                    .SendAsync(
                        CreateHttpRequest(path, body, headers),
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken)
                    .ConfigureAwait(false);
                var streamAsync = await response.Content.ReadAsStreamAsync(cancellationToken);
                var stream = new StreamReader(streamAsync);
                stream.Peek();

                return stream;
            });
        if (policyResult.Outcome == OutcomeType.Successful)
        {
            return policyResult.Result;
        }

        throw policyResult.FinalException;
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
