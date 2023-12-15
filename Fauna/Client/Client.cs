using System.Text;
using Fauna.Constants;
using Fauna.Serialization;

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
        Query query,
        QueryOptions? queryOptions = null)
    {
        var finalOptions = QueryOptions.GetFinalQueryOptions(_config.DefaultQueryOptions, queryOptions);
        var headers = GetRequestHeaders(finalOptions);

        using var stream = new MemoryStream();
        Serialize(stream, query);

        var queryResponse = await _connection.DoPostAsync<T>(QueryUriPath, stream, headers);

        if (queryResponse is QueryFailure failure)
        {
            throw new FaunaException(failure, "Query failure");
        }
        else
        {
            LastSeenTxn = queryResponse.LastSeenTxn;
        }

        return (QuerySuccess<T>)queryResponse;
    }

    private static void Serialize(Stream stream, Query query)
    {
        using var writer = new Utf8FaunaWriter(stream);
        writer.WriteStartObject();
        writer.WriteFieldName("query");
        query.Serialize(writer);
        writer.WriteEndObject();
        writer.Flush();
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
}
