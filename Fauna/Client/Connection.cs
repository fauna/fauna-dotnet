namespace Fauna;

public class Connection : IConnection
{
    private readonly RequestBuilder _requestBuilder;
    private readonly HttpClient _httpClient;

    private Connection(Builder builder)
    {
        _requestBuilder = RequestBuilder.CreateBuilder()
            .SetFaunaConfig(builder.FaunaConfig)
            .Build();
        _httpClient = builder.HttpClient;
    }

    public Task<HttpResponseMessage> PerformRequestAsync(string fql)
    {
        var request = _requestBuilder.BuildRequest(fql);
        return _httpClient.SendAsync(request);
    }

    public static Builder CreateBuilder()
    {
        return new Builder();
    }

    public class Builder
    {
        public ClientConfig? FaunaConfig { get; private set; }
        public HttpClient? HttpClient { get; private set; }

        public Builder SetFaunaConfig(ClientConfig faunaConfig)
        {
            FaunaConfig = faunaConfig;
            return this;
        }

        public Builder SetHttpClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
            return this;
        }

        public Connection Build()
        {
            return new Connection(this);
        }
    }
}
