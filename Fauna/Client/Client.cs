using Fauna.Constants;

namespace Fauna;

public class Client
{
    private const string QueryUriPath = "/query/1";

    private readonly ClientConfig _config;
    private readonly IConnection _connection;

    public long LastSeenTxn { get; private set; }

    public Client(string secret) :
        this(new ClientConfig(secret))
    {
    }

    public Client(ClientConfig config) :
        this(config, new Connection(config.Endpoint, config.ConnectionTimeout))
    {
    }

    public Client(ClientConfig config, IConnection connection)
    {
        _config = config;
        _connection = connection;
    }

    public async Task<QuerySuccess<T>> QueryAsync<T>(
        string fql,
        QueryOptions? queryOptions = null) where T : class
    {
        if (string.IsNullOrEmpty(fql)) throw new ArgumentException("The provided FQL query is null.");

        var finalOptions = QueryOptions.GetFinalQueryOptions(_config.DefaultQueryOptions, queryOptions);
        var headers = GetRequestHeaders(finalOptions);

        var response = await _connection.DoPostAsync(QueryUriPath, fql, headers);

        var queryResponse = await GetQueryResponseAsync<T>(response);

        if (queryResponse is QueryFailure failure)
        {
            throw new FaunaException(failure, "Query failure");
        }

        return (QuerySuccess<T>)queryResponse;
    }

    private Dictionary<string, string> GetRequestHeaders(QueryOptions? queryOptions)
    {
        var headers = new Dictionary<string, string>
        {

            { Headers.Authorization, $"Bearer {_config.Secret}"},
            { Headers.Format, "tagged" },
            { Headers.Driver, "C#" }
        };

        if (LastSeenTxn > long.MinValue)
        {
            headers.Add(Headers.LastTxnTs, LastSeenTxn.ToString());
        }

        if (queryOptions != null)
        {
            if (queryOptions.QueryTimeout.HasValue)
            {
                headers.Add(
                    Headers.QueryTimeoutMs,
                    queryOptions.QueryTimeout.Value.TotalMilliseconds.ToString());
            }

            if (queryOptions.QueryTags != null)
            {
                headers.Add(Headers.QueryTags, EncodeQueryTags(queryOptions.QueryTags));
            }

            if (!string.IsNullOrEmpty(queryOptions.TraceParent))
            {
                headers.Add(Headers.TraceParent, queryOptions.TraceParent);
            }

            if (queryOptions.Linearized != null)
            {
                headers.Add(Headers.Linearized, queryOptions.Linearized.ToString()!);
            }

            if (queryOptions.TypeCheck != null)
            {
                headers.Add(Headers.TypeCheck, queryOptions.TypeCheck.ToString()!);
            }
        }

        return headers;
    }

    private static string EncodeQueryTags(Dictionary<string, string> tags)
    {
        return string.Join(",", tags.Select(entry => entry.Key + "=" + entry.Value));
    }

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

            LastSeenTxn = queryResponse.LastSeenTxn;
        }

        return queryResponse;
    }
}
