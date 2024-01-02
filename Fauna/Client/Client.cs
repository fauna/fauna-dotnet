using Fauna.Constants;
using Fauna.Exceptions;
using Fauna.Serialization;

namespace Fauna;

public class Client
{
    private const string QueryUriPath = "/query/1";

    private readonly ClientConfig config;
    private readonly IConnection connection;
    // FIXME(matt) look at moving to a database context which wraps client, perhaps
    private readonly SerializationContext serializationCtx;

    public long LastSeenTxn { get; private set; }

    public Client(string secret) :
        this(new ClientConfig(secret))
    {
    }

    public Client(ClientConfig config) :
        this(config, new Connection(config.Endpoint, config.ConnectionTimeout, config.MaxRetries, config.MaxBackoff))
    {
    }

    public Client(ClientConfig config, IConnection connection)
    {
        this.config = config;
        this.connection = connection;
        this.serializationCtx = new SerializationContext();
    }

    public async Task<QuerySuccess<T>> QueryAsync<T>(
        Query query,
        QueryOptions? queryOptions = null)
    {
        if (query == null)
        {
            throw new ClientException("Query cannot be null");
        }

        var finalOptions = QueryOptions.GetFinalQueryOptions(config.DefaultQueryOptions, queryOptions);
        var headers = GetRequestHeaders(finalOptions);

        using var stream = new MemoryStream();
        Serialize(stream, query);

        using var httpResponse = await connection.DoPostAsync(QueryUriPath, stream, headers);
        var queryResponse = await QueryResponse.GetFromHttpResponseAsync<T>(httpResponse);

        if (queryResponse is QueryFailure failure)
        {
            string FormatMessage(string errorType) => $"{errorType}: {failure.ErrorInfo.Message}";

            throw failure.ErrorInfo.Code switch
            {
                // Auth Errors
                "unauthorized" => new AuthenticationException(failure, FormatMessage("Unauthorized")),
                "forbidden" => new AuthorizationException(failure, FormatMessage("Forbidden")),

                // Query Errors
                "invalid_query" or
                "invalid_function_definition" or
                "invalid_identifier" or
                "invalid_syntax" or
                "invalid_type" => new QueryCheckException(failure, FormatMessage("Invalid Query")),
                "invalid_argument" => new QueryRuntimeException(failure, FormatMessage("Invalid Argument")),
                "abort" => new AbortException(failure, FormatMessage("Abort")),

                // Request/Transaction Errors
                "invalid_request" => new InvalidRequestException(failure, FormatMessage("Invalid Request")),
                "contended_transaction" => new ContendedTransactionException(failure, FormatMessage("Contended Transaction")),

                // Capacity Errors
                "limit_exceeded" => new ThrottlingException(failure, FormatMessage("Limit Exceeded")),
                "time_limit_exceeded" => new QueryTimeoutException(failure, FormatMessage("Time Limit Exceeded")),

                // Server/Network Errors
                "internal_error" => new ServiceException(failure, FormatMessage("Internal Error")),
                "timeout" or
                "time_out" => new QueryTimeoutException(failure, FormatMessage("Timeout")),
                "bad_gateway" => new NetworkException(FormatMessage("Bad Gateway")),
                "gateway_timeout" => new NetworkException(FormatMessage("Gateway Timeout")),

                _ => new FaunaException(FormatMessage("Unexpected Error")),
            };
        }
        else
        {
            LastSeenTxn = queryResponse.LastSeenTxn;
        }

        return (QuerySuccess<T>)queryResponse;
    }

    private void Serialize(Stream stream, Query query)
    {
        using var writer = new Utf8FaunaWriter(stream);
        writer.WriteStartObject();
        writer.WriteFieldName("query");
        query.Serialize(serializationCtx, writer);
        writer.WriteEndObject();
        writer.Flush();
    }

    private Dictionary<string, string> GetRequestHeaders(QueryOptions? queryOptions)
    {
        var headers = new Dictionary<string, string>
        {

            { Headers.Authorization, $"Bearer {config.Secret}"},
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
