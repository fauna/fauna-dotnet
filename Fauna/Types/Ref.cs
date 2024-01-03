namespace Fauna.Types;

/// <summary>
/// Represents a document ref.
/// </summary>
public class Ref
{
    /// <summary>
    /// Gets the string value of the ref id.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets the collection to which the ref belongs.
    /// </summary>
    public Module Collection { get; set; }
}
