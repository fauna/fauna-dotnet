using System.Text.Json.Serialization;
using static Fauna.Constants.ResponseFields;

namespace Fauna;

/// <summary>
/// Contains detailed information about an error in a query response.
/// </summary>
public readonly struct ErrorInfo
{
    /// <summary>
    /// The error code when a query fails.
    /// </summary>
    [JsonPropertyName(Error_CodeFieldName)]
    public string Code { get; init; }

    /// <summary>
    /// The detailed message describing the cause of the error.
    /// </summary>
    [JsonPropertyName(Error_MessageFieldName)]
    public string Message { get; init; }

    /// <summary>
    /// The constraint failures that occurred during the query.
    /// </summary>
    [JsonPropertyName(Error_ConstraintFailuresFieldName)]
    public object ConstraintFailures { get; init; }

    /// <summary>
    /// The information about an abort operation within a transaction.
    /// </summary>
    [JsonPropertyName(Error_AbortFieldName)]
    public object Abort { get; init; }
}
