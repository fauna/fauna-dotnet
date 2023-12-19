using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text.Json;

namespace Fauna;

public class Connection : IConnection
{
    private readonly Uri _endpoint;
    private readonly HttpClient _httpClient;

    public Connection(Uri endpoint, TimeSpan connectionTimeout)
    {
        _endpoint = endpoint;
        _httpClient = new HttpClient()
        {
            BaseAddress = endpoint,
            Timeout = connectionTimeout
        };
    }

    public async Task<QueryResponse> DoPostAsync<T>(
        string path,
        Stream body,
        Dictionary<string, string> headers)
    {
        string FormatMessage(string errorType, string message) => $"{errorType}: {message}";

        try
        {
            body.Position = 0;
            var request = new HttpRequestMessage()
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

            var response = await _httpClient.SendAsync(request);

            return await QueryResponse.GetFromHttpResponseAsync<T>(response);
        }
        catch (HttpRequestException ex)
        {
            throw new FaunaNetworkException(FormatMessage("Network Error", ex.Message), ex);
        }
        catch (TimeoutException ex)
        {
            throw new FaunaClientException(FormatMessage("Operation Timed Out", ex.Message), ex);
        }
        catch (TaskCanceledException ex)
        {
            if (!ex.CancellationToken.IsCancellationRequested)
            {
                throw new FaunaClientException(FormatMessage("Operation Canceled", ex.Message), ex);
            }
            else
            {
                throw new FaunaClientException(FormatMessage("Operation Timed Out", ex.Message), ex);
            }
        }
        catch (ArgumentNullException ex)
        {
            throw new FaunaClientException(FormatMessage("Null Argument", ex.Message), ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new FaunaClientException(FormatMessage("Invalid Operation", ex.Message), ex);
        }
        catch (JsonException ex)
        {
            throw new FaunaProtocolException(FormatMessage("Response Parsing Failed", ex.Message), ex);
        }
        catch (AuthenticationException ex)
        {
            throw new FaunaClientException(FormatMessage("Authentication Failed", ex.Message), ex);
        }
        catch (NotSupportedException ex)
        {
            throw new FaunaClientException(FormatMessage("Not Supported Operation", ex.Message), ex);
        }
        catch (Exception ex)
        {
            throw new FaunaBaseException(FormatMessage("Unexpected Error", ex.Message), ex);
        }
    }
}
