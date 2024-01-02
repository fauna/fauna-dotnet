using Fauna.Exceptions;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using AuthenticationException = System.Security.Authentication.AuthenticationException;

namespace Fauna;

public class Connection : IConnection
{
    private readonly Uri _endpoint;
    private readonly HttpClient _httpClient;
    private readonly int _maxRetries;
    private readonly TimeSpan _maxBackoff;

    public Connection(Uri endpoint, TimeSpan connectionTimeout, int maxRetries, TimeSpan maxBackoff)
    {
        _endpoint = endpoint;
        _maxRetries = maxRetries;
        _maxBackoff = maxBackoff;
        _httpClient = new HttpClient()
        {
            BaseAddress = endpoint,
            Timeout = connectionTimeout
        };
    }

    public async Task<HttpResponseMessage> DoPostAsync(
        string path,
        Stream body,
        Dictionary<string, string> headers)
    {
        string FormatMessage(string errorType, string message) => $"{errorType}: {message}";

        int attempt = 0;
        while (true)
        {
            try
            {
                var request = CreateHttpRequest(path, body, headers);
                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.TooManyRequests && attempt < _maxRetries)
                {
                    await ApplyExponentialBackoff(attempt);
                    attempt++;
                }
                else
                {
                    return response;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new NetworkException(FormatMessage("Network Error", ex.Message), ex);
            }
            catch (TaskCanceledException ex)
            {
                if (!ex.CancellationToken.IsCancellationRequested)
                {
                    throw new ClientException(FormatMessage("Operation Canceled", ex.Message), ex);
                }
                else
                {
                    throw new ClientException(FormatMessage("Operation Timed Out", ex.Message), ex);
                }
            }
            catch (ArgumentNullException ex)
            {
                throw new ClientException(FormatMessage("Null Argument", ex.Message), ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new ClientException(FormatMessage("Invalid Operation", ex.Message), ex);
            }
            catch (JsonException ex)
            {
                throw new ProtocolException(FormatMessage("Response Parsing Failed", ex.Message), ex);
            }
            catch (AuthenticationException ex)
            {
                throw new ClientException(FormatMessage("Authentication Failed", ex.Message), ex);
            }
            catch (NotSupportedException ex)
            {
                throw new ClientException(FormatMessage("Not Supported Operation", ex.Message), ex);
            }
            catch (Exception ex)
            {
                throw new FaunaException(FormatMessage("Unexpected Error", ex.Message), ex);
            }
        }
    }

    private HttpRequestMessage CreateHttpRequest(string path, Stream body, Dictionary<string, string> headers)
    {
        body.Position = 0;
        var request = new HttpRequestMessage
        {
            Content = new StreamContent(body),
            Method = HttpMethod.Post,
            RequestUri = new Uri(_endpoint, path)
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

        foreach (var header in headers)
        {
            request.Headers.Add(header.Key, header.Value);
        }

        return request;
    }

    private async Task ApplyExponentialBackoff(int attempt)
    {
        var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
        await Task.Delay(delay > _maxBackoff ? _maxBackoff : delay);
    }
}
