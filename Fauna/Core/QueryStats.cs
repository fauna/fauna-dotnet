using System.Text.Json.Serialization;
using static Fauna.Core.ResponseFields;

namespace Fauna.Core;

/// <summary>
/// Contains statistics related to the execution of a query in the Fauna database.
/// </summary>
public readonly struct QueryStats
{
    /// <summary>
    /// The number of compute operations consumed by the query.
    /// </summary>
    [JsonPropertyName(Stats_ComputeOpsFieldName)]
    public int ComputeOps { get; init; }

    /// <summary>
    /// The number of read operations consumed by the query.
    /// </summary>
    [JsonPropertyName(Stats_ReadOps)]
    public int ReadOps { get; init; }

    /// <summary>
    /// The number of write operations consumed by the query.
    /// </summary>
    [JsonPropertyName(Stats_WriteOps)]
    public int WriteOps { get; init; }

    /// <summary>
    /// The query processing time in milliseconds.
    /// </summary>
    [JsonPropertyName(Stats_QueryTimeMs)]
    public int QueryTimeMs { get; init; }

    /// <summary>
    /// The write contention retry count.
    /// </summary>
    [JsonPropertyName(Stats_ContentionRetries)]
    public int ContentionRetries { get; init; }

    /// <summary>
    /// The amount of data read from storage, in bytes.
    /// </summary>
    [JsonPropertyName(Stats_StorageBytesRead)]
    public int StorageBytesRead { get; init; }

    /// <summary>
    /// The amount of data written to storage, in bytes.
    /// </summary>
    [JsonPropertyName(Stats_StorageBytesWrite)]
    public int StorageBytesWrite { get; init; }

    /// <summary>
    /// The types of operations that were limited or approaching rate limits.
    /// </summary>
    [JsonPropertyName(Stats_RateLimitsHit)]
    public List<string> RateLimitsHit { get; init; }

    /// <summary>
    /// Processing time in milliseconds, only returned on Events.
    /// </summary>
    [JsonPropertyName(Stats_ProcessingTimeMs)]
    public int? ProcessingTimeMs { get; init; }

    /// <summary>
    /// Returns a string representation of the query statistics.
    /// </summary>
    /// <returns>A string detailing the query execution statistics.</returns>
    public override string ToString()
    {
        return $"compute: {ComputeOps}, read: {ReadOps}, write: {WriteOps}, " +
               $"queryTime: {QueryTimeMs}, retries: {ContentionRetries}, " +
               $"storageRead: {StorageBytesRead}, storageWrite: {StorageBytesWrite}, " +
               $"{(ProcessingTimeMs.HasValue ? $"processingTime: {ProcessingTimeMs}, " : "")}" +
               $"limits: [{string.Join(',', RateLimitsHit)}]";
    }
}
