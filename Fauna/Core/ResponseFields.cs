namespace Fauna.Core;

/// <summary>
/// Contains constant values for the response field names returned by Fauna API queries.
/// </summary>
internal readonly struct ResponseFields
{
    #region Top-level fields

    /// <summary>
    /// Field name for the main data content of the response.
    /// </summary>
    public const string DataFieldName = "data";

    /// <summary>
    /// Field name for the transaction timestamp of the last transaction seen by the request.
    /// </summary>
    public const string LastSeenTxnFieldName = "txn_ts";

    /// <summary>
    /// Field name for the stream cursor of the response.
    /// </summary>
    public const string CursorFieldName = "cursor";

    /// <summary>
    /// Field name for static type information in the response.
    /// </summary>
    public const string StaticTypeFieldName = "static_type";

    /// <summary>
    /// Field name for statistical information about the query execution.
    /// </summary>
    public const string StatsFieldName = "stats";

    /// <summary>
    /// Field name for the schema version of the database at the time of query execution.
    /// </summary>
    public const string SchemaVersionFieldName = "schema_version";

    /// <summary>
    /// Field name for the summary information about the query execution.
    /// </summary>
    public const string SummaryFieldName = "summary";

    /// <summary>
    /// Field name for query tags associated with the request, used in logging and monitoring.
    /// </summary>
    public const string QueryTagsFieldName = "query_tags";

    /// <summary>
    /// Field name for error information if the query fails.
    /// </summary>
    public const string ErrorFieldName = "error";

    /// <summary>
    /// Field name for pagination information.
    /// </summary>
    public const string HasNextFieldName = "has_next";

    /// <summary>
    /// Field name for array of events.
    /// </summary>
    public const string EventsFieldName = "events";

    #endregion

    #region "stats" block

    /// <summary>
    /// Field name for the number of compute operations consumed by the query.
    /// </summary>
    public const string Stats_ComputeOpsFieldName = "compute_ops";

    /// <summary>
    /// Field name for the number of read operations consumed by the query.
    /// </summary>
    public const string Stats_ReadOps = "read_ops";

    /// <summary>
    /// Field name for the number of write operations consumed by the query.
    /// </summary>
    public const string Stats_WriteOps = "write_ops";

    /// <summary>
    /// Field name for the query processing time in milliseconds.
    /// </summary>
    public const string Stats_QueryTimeMs = "query_time_ms";

    /// <summary>
    /// Field name for the write contention retry count.
    /// </summary>
    public const string Stats_ContentionRetries = "contention_retries";

    /// <summary>
    /// Field name for the amount of data read from storage, in bytes.
    /// </summary>
    public const string Stats_StorageBytesRead = "storage_bytes_read";

    /// <summary>
    /// Field name for the amount of data written to storage, in bytes.
    /// </summary>
    public const string Stats_StorageBytesWrite = "storage_bytes_write";

    /// <summary>
    /// Field name for the types of operations that were limited or approaching rate limits.
    /// </summary>
    public const string Stats_RateLimitsHit = "rate_limits_hit";

    /// <summary>
    /// Field name for the processing time in milliseconds.
    /// </summary>
    public const string Stats_ProcessingTimeMs = "processing_time_ms";

    #endregion

    #region "error" block

    /// <summary>
    /// Field name for the error code when a query fails.
    /// </summary>
    public const string Error_CodeFieldName = "code";

    /// <summary>
    /// Field name for the detailed message describing the cause of the error.
    /// </summary>
    public const string Error_MessageFieldName = "message";

    /// <summary>
    /// Field name for constraint failures that occurred during the query.
    /// </summary>
    public const string Error_ConstraintFailuresFieldName = "constraint_failures";

    /// <summary>
    /// Field name for the message in constraint failures.
    /// </summary>
    public const string Error_ConstraintFailuresMessageFieldName = "message";

    /// <summary>
    /// Field name for the name in constraint failures.
    /// </summary>
    public const string Error_ConstraintFailuresNameFieldName = "name";

    /// <summary>
    /// Field name for the paths in constraint failures.
    /// </summary>
    public const string Error_ConstraintFailuresPathsFieldName = "paths";

    /// <summary>
    /// Field name for information about an abort operation within a transaction.
    /// </summary>
    public const string Error_AbortFieldName = "abort";

    #endregion
}
