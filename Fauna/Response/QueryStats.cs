using System.Text.Json.Serialization;
using static Fauna.Constants.ResponseFields;

namespace Fauna;

public readonly struct QueryStats
{
    [JsonPropertyName(Stats_ComputeOpsFieldName)]
    public int ComputeOps { get; init; }

    [JsonPropertyName(Stats_ReadOps)]
    public int ReadOps { get; init; }

    [JsonPropertyName(Stats_WriteOps)]
    public int WriteOps { get; init; }

    [JsonPropertyName(Stats_QueryTimeMs)]
    public int QueryTimeMs { get; init; }

    [JsonPropertyName(Stats_ContentionRetries)]
    public int ContentionRetries { get; init; }

    [JsonPropertyName(Stats_StorageBytesRead)]
    public int StorageBytesRead { get; init; }

    [JsonPropertyName(Stats_StorageBytesWrite)]
    public int StorageBytesWrite { get; init; }

    [JsonPropertyName(Stats_RateLimitsHit)]
    public List<string> RateLimitsHit { get; init; }

    public override readonly string ToString()
    {
        return $"compute: {ComputeOps}, read: {ReadOps}, write: {WriteOps}, " +
               $"queryTime: {QueryTimeMs}, retries: {ContentionRetries}, " +
               $"storageRead: {StorageBytesRead}, storageWrite: {StorageBytesWrite}, " +
               $"limits: [{string.Join(',', RateLimitsHit)}]";
    }
}
