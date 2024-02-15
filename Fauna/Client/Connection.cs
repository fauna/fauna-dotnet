using Fauna.Exceptions;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Polly;
using AuthenticationException = System.Security.Authentication.AuthenticationException;

namespace Fauna;

/// <summary>
/// An HTTP Client wrapper.
/// </summary>
public class Connection : IConnection
{
    private readonly Configuration _cfg;

    /// <summary>
    /// Initializes a new instance of the Connection class.
    /// </summary>
    /// <param name="configuration">The client configuration to use.</param>
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
                .ExecuteAndCaptureAsync(async () => await _cfg.HttpClient.SendAsync(CreateHttpRequest(path, body, headers), cancel))
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
}
