namespace Fauna.Types;

/// <summary>
/// Represents a document ref that has a "name" instead of an "id". For example, a Role document reference is
/// represented as a NamedRef.
/// </summary>
public class NamedRef
{
    public NamedRef(string name, Module collection)
    {
        Name = name;
        Collection = collection;
    }

    /// <summary>
    /// Gets the string value of the ref name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the collection to which the ref belongs.
    /// </summary>
    public Module Collection { get; }
}
