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

    public async Task<QueryResponse> DoPostAsync<T>(
        string path,
        Stream body,
        Dictionary<string, string> headers)
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
}
