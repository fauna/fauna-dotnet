namespace Fauna;

public class Client
{
    private readonly IConnection _connection;

    public Client(string secret, HttpClient? httpClient = null) :
        this(new ClientConfig { Secret = secret }, httpClient)
    {
    }

    public Client(ClientConfig clientConfig, HttpClient? httpClient = null)
    {
        // Initialize the connection
        _connection = new Connection(
            clientConfig,
            httpClient ?? new HttpClient()
            {
                BaseAddress = clientConfig.Endpoint,
                Timeout = clientConfig.ConnectionTimeout
            });
    }

    public async Task<string> QueryAsync(string fql)
    {
        if (fql == null) throw new ArgumentException("The provided FQL query is null.");

        var response = await _connection.PerformRequestAsync(fql);

        return ProcessResponse(response);
    }

    public async Task<string> QueryAsync(Query fql)
    {
        throw new NotImplementedException();
    }

    // ProcessResponse method
    private string ProcessResponse(HttpResponseMessage response)
    {
        int statusCode = (int)response.StatusCode;
        var body = response.Content.ReadAsStringAsync().Result;

        // Error handling

        return body;
    }
}
