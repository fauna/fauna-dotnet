namespace Fauna;

public class Connection : IConnection
{
    private readonly RequestBuilder _requestBuilder;
    private readonly HttpClient _httpClient;

    internal Connection(ClientConfig config, HttpClient httpClient)
    {
        _requestBuilder = new RequestBuilder(config);
        _httpClient = httpClient;
    }

    public Task<HttpResponseMessage> PerformRequestAsync(string fql)
    {
        var request = _requestBuilder.BuildRequest(fql);
        return _httpClient.SendAsync(request);
    }
}
