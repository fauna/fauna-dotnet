using System.Text.Json.Serialization;
using static Fauna.Core.ResponseFields;

namespace Fauna.Exceptions;

/// <summary>
/// A class representing a  <see href="https://docs.fauna.com/fauna/current/reference/fsl/check/">constraint failure</see> from Fauna.
/// </summary>
public class ConstraintFailure
{
    /// <summary>
    /// Initializes a new <see cref="ConstraintFailure"/>.
    /// </summary>
    /// <param name="message">The message describing the constraint failure.</param>
    /// <param name="name">The name of the constraint failure.</param>
    /// <param name="paths">The paths for the constraint failure.</param>
    public ConstraintFailure(string message, string name, object[][]? paths)
    {
        Message = message;
        Name = name;
        Paths = paths;
    }

    /// <summary>
    /// The constraint failure message describing the specific check that failed.
    /// </summary>
    [JsonPropertyName(Error_ConstraintFailuresMessageFieldName)]
    public string Message { get; set; }

    /// <summary>
    /// The constraint failure name.
    /// </summary>
    [JsonPropertyName(Error_ConstraintFailuresNameFieldName)]
    public string Name { get; set; }

    /// <summary>
    /// The constraint failure paths.
    /// </summary>
    [JsonPropertyName(Error_ConstraintFailuresPathsFieldName)]
    public object[][]? Paths { get; set; }

}
