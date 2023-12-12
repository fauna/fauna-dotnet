namespace Fauna.Constants;

internal readonly struct ResponseFields
{
    // Top-level fields
    public const string DataFieldName = "data";
    public const string LastSeenTxnFieldName = "txn_ts";
    public const string StaticTypeFieldName = "static_type";
    public const string StatsFieldName = "stats";
    public const string SchemaVersionFieldName = "schema_version";
    public const string SummaryFieldName = "summary";
    public const string QueryTagsFieldName = "query_tags";
    public const string ErrorFieldName = "error";

    // "stats" block
    public const string Stats_ComputeOpsFieldName = "compute_ops";
    public const string Stats_ReadOps = "read_ops";
    public const string Stats_WriteOps = "write_ops";
    public const string Stats_QueryTimeMs = "query_time_ms";
    public const string Stats_ContentionRetries = "contention_retries";
    public const string Stats_StorageBytesRead = "storage_bytes_read";
    public const string Stats_StorageBytesWrite = "storage_bytes_write";
    public const string Stats_RateLimitsHit = "rate_limits_hit";

    // "error" block
    public const string Error_CodeFieldName = "code";
    public const string Error_MessageFieldName = "message";
    public const string Error_ConstraintFailuresFieldName = "constraint_failures";
    public const string Error_AbortFieldName = "abort";
}
