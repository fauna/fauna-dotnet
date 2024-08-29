namespace Fauna.Core;

/// <summary>
/// Contains constant values for HTTP header names used in Fauna API requests.
/// </summary>
internal readonly struct Headers
{
    /// <summary>
    /// Header for the authorization token in API requests.
    /// </summary>
    public const string Authorization = "Authorization";

    /// <summary>
    /// Header indicating the minimum snapshot time for the query execution based on the highest transaction timestamp observed by the client.
    /// </summary>
    public const string LastTxnTs = "X-Last-Txn-Ts";

    /// <summary>
    /// Header to enforce strictly serialized execution of the query, affecting read-only transactions.
    /// </summary>
    public const string Linearized = "X-Linearized";

    /// <summary>
    /// Header indicating the maximum number of retries for a transaction due to contention failure before returning an error.
    /// </summary>
    public const string MaxContentionRetries = "X-Max-Contention-Retries";

    /// <summary>
    /// Header specifying the query timeout in milliseconds.
    /// </summary>
    public const string QueryTimeoutMs = "X-Query-Timeout-Ms";

    /// <summary>
    /// Header to enable or disable type checking of the query before evaluation.
    /// </summary>
    public const string TypeCheck = "X-Typecheck";

    /// <summary>
    /// Header for passing custom, string-encoded tags for request identification in logs and responses.
    /// </summary>
    public const string QueryTags = "X-Query-Tags";

    /// <summary>
    /// Header for the trace parent identifier in distributed tracing systems.
    /// </summary>
    public const string TraceParent = "Traceparent";

    /// <summary>
    /// Header indicating the driver used for the API request.
    /// </summary>
    public const string Driver = "X-Driver";

    /// <summary>
    /// Header for specifying the environment of the driver used in the API request.
    /// </summary>
    public const string DriverEnv = "X-Driver-Env";

    /// <summary>
    /// Header for specifying the encoded format for query arguments and response data.
    /// Options are 'simple' and 'tagged'. 'Simple' is the default format.
    /// </summary>
    public const string Format = "X-Format";
}
