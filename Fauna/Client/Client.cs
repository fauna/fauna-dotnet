using Fauna.Constants;
using Fauna.Exceptions;
using Fauna.Linq;
using Fauna.Mapping;
using Fauna.Serialization;
using System.Globalization;
using System.Net;

namespace Fauna;

/// <summary>
/// Represents a client for interacting with a Fauna.
/// </summary>
public class Client : BaseClient, IDisposable
{
    private const string QueryUriPath = "/query/1";

    private readonly Configuration _config;
    private readonly IConnection _connection;

    private readonly MappingContext _defaultCtx = new();
    private readonly Dictionary<Type, DataContext> _dbCtxs = new();

    private bool _disposed;

    internal override MappingContext MappingCtx { get => _defaultCtx; }

    /// <summary>
    /// Gets the timestamp of the last transaction seen by this client.
    /// </summary>
    public long LastSeenTxn { get; private set; }

    /// <summary>
    /// Initializes a new instance of a Client with a secret.
    /// </summary>
    /// <param name="secret">The secret key for authentication.</param>
    public Client(string secret) :
        this(new Configuration(secret))
    {
    }

    /// <summary>
    /// Initializes a new instance of the Client with a custom <see cref="Configuration"/>.
    /// </summary>
    /// <param name="config">The configuration settings for the client.</param>
    public Client(Configuration config)
    {
        _config = config;
        _connection = new Connection(config);
    }

    /// <summary>
    /// Create and return a new database context which uses the <see cref="Client"/> instance.
    /// </summary>
    /// <typeparam name="DB">The DataContext subtype to instantiate.</typeparam>
    /// <returns>An instance of <typeparamref name="DB"/>.</returns>
    public DB DataContext<DB>() where DB : DataContext
    {
        var dbCtxType = typeof(DB);
        DataContext? ctx;
        lock (_dbCtxs)
        {
            if (!_dbCtxs.TryGetValue(dbCtxType, out ctx))
            {
                var builder = new DataContextBuilder<DB>();
                ctx = builder.Build(this);
                _dbCtxs[dbCtxType] = ctx;
            }
        }

        return (DB)ctx;
    }
    
    /// <summary>
    /// Disposes the resources used by the <see cref="Client"/> class.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // A finalizer: https://stackoverflow.com/questions/151051/when-should-i-use-gc-suppressfinalize
    ~Client()
    {
        Dispose(false);
    }

    internal override async Task<QuerySuccess<T>> QueryAsyncInternal<T>(
        Query query,
        IDeserializer<T> deserializer,
        MappingContext ctx,
        QueryOptions? queryOptions,
        CancellationToken cancel)
    {
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        var finalOptions = QueryOptions.GetFinalQueryOptions(_config.DefaultQueryOptions, queryOptions);
        var headers = GetRequestHeaders(finalOptions);

        using var stream = new MemoryStream();
        Serialize(stream, query, ctx);

        using var httpResponse = await _connection.DoPostAsync(QueryUriPath, stream, headers, cancel);
        var body = await httpResponse.Content.ReadAsStringAsync(cancel);
        var res = QueryResponse.GetFromResponseBody<T>(ctx, deserializer, httpResponse.StatusCode, body);
        switch (res)
        {
            case QuerySuccess<T> success:
                LastSeenTxn = res.LastSeenTxn;
                return success;
            case QueryFailure failure:
                throw ExceptionFactory.FromQueryFailure(ctx, failure);
            default:
                throw ExceptionFactory.FromRawResponse(body, httpResponse);
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

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _connection.Dispose();
            GC.SuppressFinalize(this);
        }
        _disposed = true;
    }
}
