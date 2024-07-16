namespace Fauna.Types;

/// <summary>
/// Represents a document ref.
/// </summary>
public class Ref
{
    public Ref(string id, Module collection)
    {
        Id = id;
        Collection = collection;
    }

    /// <summary>
    /// Gets the string value of the ref id.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the collection to which the ref belongs.
    /// </summary>
    public Module Collection { get; }
}
