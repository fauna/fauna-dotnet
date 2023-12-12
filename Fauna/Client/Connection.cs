using System.Net.Http.Headers;
using System.Net.Http.Json;

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

    public async Task<HttpResponseMessage> DoPostAsync(
        string path,
        string body,
        Dictionary<string, string> headers)
    {
        // TODO: Serialize body here
        var queryObj = new
        {
            query = new
            {
                fql = new[] { body }
            },
            arguments = new { }
        };

        var request = new HttpRequestMessage()
        {
            Content = JsonContent.Create(queryObj),
            Method = HttpMethod.Post,
            RequestUri = new Uri(_endpoint, path)
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));

        foreach (var header in headers)
        {
            request.Headers.Add(header.Key, header.Value);
        }

        return await _httpClient.SendAsync(request);
    }
}
