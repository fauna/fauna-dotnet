using Fauna.Configuration;

namespace Fauna;

public class Client
{
    private readonly IConnection _connection;
    private const int DefaultTimeout = 5;

    public Client(Config faunaConfig, HttpClient? httpClient = null)
    {
        // Initialize the connection
        _connection = Connection.CreateBuilder()
            .SetFaunaConfig(faunaConfig)
            .SetHttpClient(httpClient ?? new HttpClient()
            {
                Timeout = TimeSpan.FromSeconds(DefaultTimeout)
            })
            .Build();
    }

    public async Task<T?> QueryAsync<T>(string fql) where T : class
    {
        if (fql == null) throw new ArgumentException("The provided FQL query is null.");

        var response = await _connection.PerformRequestAsync(fql);

        //ProcessResponse<T>(response)

        return null;
    }

    // ProcessResponse method
    private T? ProcessResponse<T>(HttpResponseMessage response) where T : class
    {
        int statusCode = (int)response.StatusCode;
        var body = response.Content.ReadAsStringAsync().Result;

        // Error handling

        return null;
    }
}
