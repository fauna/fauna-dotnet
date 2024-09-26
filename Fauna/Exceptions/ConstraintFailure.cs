using System.Text.Json.Serialization;
using static Fauna.Core.ResponseFields;

namespace Fauna.Exceptions;

public class ConstraintFailure
{
    public ConstraintFailure(string message, string name, object[][]? paths)
    {
        Message = message;
        Name = name;
        Paths = paths;
    }

    [JsonPropertyName(Error_ConstraintFailuresMessageFieldName)]
    public string Message { get; set; }

    [JsonPropertyName(Error_ConstraintFailuresNameFieldName)]
    public string Name { get; set; }

    [JsonPropertyName(Error_ConstraintFailuresPathsFieldName)]
    public object[][]? Paths { get; set; }

}
