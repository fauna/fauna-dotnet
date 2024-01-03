using System.Globalization;
using Fauna.Constants;
using Fauna.Exceptions;
using Fauna.Response;
using Fauna.Serialization;

namespace Fauna;

/// <summary>
/// Represents a client for interacting with a Fauna.
/// </summary>
public class Client
{
    private const string QueryUriPath = "/query/1";

    private readonly ClientConfig _config;
    private readonly IConnection _connection;
    // FIXME(matt) look at moving to a database context which wraps client, perhaps
    private readonly SerializationContext _serializationCtx;

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
        this(config, new Connection(config.Endpoint, config.ConnectionTimeout, config.MaxRetries, config.MaxBackoff))
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
        this._serializationCtx = new SerializationContext();
    }

    /// <summary>
    /// Asynchronously executes a specified FQL query against the Fauna database and returns the typed result.
    /// </summary>
    /// <typeparam name="T">The type of the result expected from the query, corresponding to the structure of the FQL query's expected response.</typeparam>
    /// <param name="query">The FQL query object representing the query to be executed against the Fauna database.</param>
    /// <param name="queryOptions">Optional parameters to customize the query execution, such as timeout settings and custom headers.</param>
    /// <returns>A Task representing the asynchronous operation, which upon completion contains the result of the query execution as <see cref="QuerySuccess{T}"/>.</returns>
    /// <exception cref="ClientException">Thrown when client-side errors occur before sending the request to Fauna.</exception>
    /// <exception cref="AuthenticationException">Thrown when authentication fails due to invalid credentials or other authentication issues.</exception>
    /// <exception cref="AuthorizationException">Thrown when the client lacks sufficient permissions to execute the query.</exception>
    /// <exception cref="QueryCheckException">Thrown when the query has syntax errors or is otherwise malformed.</exception>
    /// <exception cref="QueryRuntimeException">Thrown when runtime errors occur during query execution, such as invalid arguments or operational failures.</exception>
    /// <exception cref="AbortException">Thrown when the FQL `abort` function is called within the query, containing the data provided during the abort operation.</exception>
    /// <exception cref="InvalidRequestException">Thrown for improperly formatted requests or requests that Fauna cannot process.</exception>
    /// <exception cref="ContendedTransactionException">Thrown when a transaction is aborted due to concurrent modification or contention issues.</exception>
    /// <exception cref="ThrottlingException">Thrown when the query exceeds established rate limits for the Fauna service.</exception>
    /// <exception cref="QueryTimeoutException">Thrown when the query execution time exceeds the specified or default timeout period.</exception>
    /// <exception cref="ServiceException">Thrown in response to internal Fauna service errors, indicating issues on the server side.</exception>
    /// <exception cref="NetworkException">Thrown for failures in network communication between the client and Fauna service.</exception>
    /// <exception cref="FaunaException">Thrown for unexpected or miscellaneous errors not covered by the other specific exception types.</exception>
    public async Task<QuerySuccess<T>> QueryAsync<T>(
        Query query,
        QueryOptions? queryOptions = null)
    {
        var queryResponse = await QueryAsyncInternal<T>(query, queryOptions);
        return (QuerySuccess<T>)queryResponse;
    }

    public async IAsyncEnumerable<Page> PaginateAsync(Query query, QueryOptions? queryOptions = null)
    {
        Page? currentPage = null;
        bool isFirstPageFetched = false;

        while (!isFirstPageFetched || currentPage?.After != null)
        {
            Query currentQuery = isFirstPageFetched && currentPage?.After is not null
                ? new QueryExpr(new QueryLiteral($"Set.paginate('{currentPage.After}')"))
                : query;

            var response = await QueryAsyncInternal<Page>(currentQuery, queryOptions);

            if (response is QueryPageSuccess pageSuccess && pageSuccess.Data is QueryPageInfo pageInfo)
            {
                currentPage = new Page(pageInfo.Data, pageInfo.After);
                isFirstPageFetched = true;

                if (currentPage is not null)
                {
                    yield return currentPage;
                }
            }
            else
            {
                throw new InvalidOperationException("Unexpected response type received.");
            }
        }
    }

    private async Task<QueryResponse> QueryAsyncInternal<T>(
        Query query,
        QueryOptions? queryOptions = null)
    {
        if (query == null)
        {
            throw new ClientException("Query cannot be null");
        }

        var finalOptions = QueryOptions.GetFinalQueryOptions(_config.DefaultQueryOptions, queryOptions);
        var headers = GetRequestHeaders(finalOptions);

        using var stream = new MemoryStream();
        Serialize(stream, query);

        var queryResponse = await _connection.DoPostAsync<T>(QueryUriPath, stream, headers);

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

        return queryResponse;
    }

    private void Serialize(Stream stream, Query query)
    {
        using var writer = new Utf8FaunaWriter(stream);
        writer.WriteStartObject();
        writer.WriteFieldName("query");
        query.Serialize(_serializationCtx, writer);
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

public static class PaginationExtensions
{
    public static async IAsyncEnumerable<T> FlattenAsync<T>(this IAsyncEnumerable<Page> pages)
    {
        await foreach (var page in pages)
        {
            foreach (var item in page.GetData<T>())
            {
                yield return item;
            }
        }
    }
}

