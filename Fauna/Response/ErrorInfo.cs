using System.Text.Json.Serialization;
using static Fauna.Constants.ResponseFields;

namespace Fauna;

public readonly struct ErrorInfo
{
    [JsonPropertyName(Error_CodeFieldName)]
    public string Code { get; init; }

    [JsonPropertyName(Error_MessageFieldName)]
    public string Message { get; init; }

    [JsonPropertyName(Error_ConstraintFailuresFieldName)]
    public object ConstraintFailures { get; init; }

    [JsonPropertyName(Error_AbortFieldName)]
    public object Abort { get; init; }
}
