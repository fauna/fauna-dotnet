using Fauna.Constants;
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Serialization;
using System.Data;
using System.Globalization;

namespace Fauna;

/// <summary>
/// Represents a client for interacting with a Fauna.
/// </summary>
public class Client : BaseClient
{
    private const string QueryUriPath = "/query/1";

    private readonly ClientConfig _config;
    private readonly IConnection _connection;

    private readonly MappingContext _defaultCtx = new();
    private readonly Dictionary<Type, DatabaseContext> _dbCtxs = new();

    protected override MappingContext MappingCtx { get => _defaultCtx; }

    /// <summary>
    /// Gets the timestamp of the last transaction seen by this client.
    /// </summary>
    public long LastSeenTxn { get; private set; }

    /// <summary>
    /// Initializes a new instance of the Client class using a secret key.
    /// </summary>
    /// <param name="secret">The secret key for authentication.</param>
    public Client(string secret) :
        this(new ClientConfig(secret))
    {
    }

    /// <summary>
    /// Initializes a new instance of the Client class using client configuration.
    /// </summary>
    /// <param name="config">The configuration settings for the client.</param>
    public Client(ClientConfig config) :
        this(config, new Connection(config.Endpoint,
                                    config.ConnectionTimeout,
                                    config.MaxRetries,
                                    config.MaxBackoff))
    {
    }

    /// <summary>
    /// Initializes a new instance of the Client class using client configuration and a custom connection.
    /// </summary>
    /// <param name="config">The configuration settings for the client.</param>
    /// <param name="connection">The custom connection to be used by the client.</param>
    public Client(ClientConfig config, IConnection connection)
    {
        this._config = config;
        this._connection = connection;
    }

    /// <summary>
    /// Create and return a new database context which uses this client.
    /// </summary>
    /// <typeparam name="DB">The DatabaseContext subtype to instantiate.</typeparam>
    /// <returns>An instance of <typeparamref name="DB"/>.</returns>
    public DB DatabaseContext<DB>() where DB : DatabaseContext
    {
        var dbCtxType = typeof(DB);
        DatabaseContext? ctx;
        lock (_dbCtxs)
        {
            if (!_dbCtxs.TryGetValue(dbCtxType, out ctx))
            {
                ctx = (DB)Activator.CreateInstance(dbCtxType)!;
                ctx.SetClient(this);
                _dbCtxs[dbCtxType] = ctx;
            }
        }

        return (DB)ctx;
    }

    internal override async Task<QuerySuccess<T>> QueryAsyncInternal<T>(
        Query query,
        IDeserializer<T> deserializer,
        MappingContext ctx,
        QueryOptions? queryOptions)
    {
        if (query == null)
        {
            throw new ClientException("Query cannot be null");
        }

        var finalOptions = QueryOptions.GetFinalQueryOptions(_config.DefaultQueryOptions, queryOptions);
        var headers = GetRequestHeaders(finalOptions);

        using var stream = new MemoryStream();
        Serialize(stream, query, ctx);

        using var httpResponse = await _connection.DoPostAsync(QueryUriPath, stream, headers);
        var queryResponse = await QueryResponse.GetFromHttpResponseAsync<T>(ctx,
                                                                            deserializer,
                                                                            httpResponse);
        switch (queryResponse)
        {
            case QuerySuccess<T> success:
                LastSeenTxn = queryResponse.LastSeenTxn;
                return success;

            case QueryFailure failure:
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
                    "abort" => new AbortException(ctx, failure, FormatMessage("Abort")),

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

            default:
                throw new InvalidOperationException("Unreachable");
        }
    }

    private void Serialize(Stream stream, Query query, MappingContext ctx)
    {
        using var writer = new Utf8FaunaWriter(stream);
        writer.WriteStartObject();
        writer.WriteFieldName("query");
        query.Serialize(ctx, writer);
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
                    queryOptions.QueryTimeout.Value.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
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
