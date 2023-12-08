using Fauna.Constants;

namespace Fauna;

public class Client
{
    private readonly IConnection _connection;

    public Client(string secret) :
        this(new ClientConfig(secret))
    {
    }

    public Client(ClientConfig config, HttpClient? httpClient = null)
    {
        var connectionHttpClient = httpClient ?? new HttpClient();
        connectionHttpClient.Timeout = config.ConnectionTimeout;

        // Initialize the connection
        _connection = new Connection(config, connectionHttpClient);
    }

    public async Task<QuerySuccess<T>> QueryAsync<T>(
        string fql,
        QueryOptions? queryOptions = null) where T : class
    {
        if (string.IsNullOrEmpty(fql)) throw new ArgumentException("The provided FQL query is null.");

        var response = await _connection.DoRequestAsync(fql, queryOptions);

        var queryResponse = await GetQueryResponseAsync<T>(response);

        if (queryResponse is QueryFailure)
        {
            throw new Exception("Query failure");
        }

        return (QuerySuccess<T>)queryResponse;
    }

    // ProcessResponse method
    private async Task<QueryResponse> GetQueryResponseAsync<T>(HttpResponseMessage response) where T : class
    {
        QueryResponse queryResponse;

        var statusCode = response.StatusCode;
        var body = await response.Content.ReadAsStringAsync();
        var headers = response.Headers;

        if (!response.IsSuccessStatusCode)
        {
            queryResponse = new QueryFailure(body);
        }
        else
        {
            queryResponse = new QuerySuccess<T>(body);

            _connection.LastSeenTxn = queryResponse.LastSeenTxn;
        }

        return queryResponse;
    }
}
