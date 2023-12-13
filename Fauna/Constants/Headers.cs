namespace Fauna.Constants;

internal readonly struct Headers
{
    public const string Authorization = "Authorization";
    public const string LastTxnTs = "X-Last-Txn-Ts";
    public const string Linearized = "X-Linearized";
    public const string MaxContentionRetries = "X-Max-Contention-Retries";
    public const string QueryTimeoutMs = "X-Query-Timeout-Ms";
    public const string TypeCheck = "X-Typecheck";
    public const string QueryTags = "X-Query-Tags";
    public const string TraceParent = "Traceparent";
    public const string Driver = "X-Driver";
    public const string DriverEnv = "X-Driver-Env";
    public const string Format = "X-Format";
}
