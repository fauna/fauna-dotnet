namespace Fauna.Types;

/// <summary>
/// Represents a document that has a "name" instead of an "id". For example, a Role document is represented as a
/// NamedDocument.
/// </summary>
public sealed class NamedDocument : BaseDocument
{

    /// <summary>
    /// Gets the string value of the document name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the NamedDocument class with the specified name, coll, and ts.
    /// </summary>
    /// <param name="name">The string value of the document name.</param>
    /// <param name="coll">The module to which the document belongs.</param>
    /// <param name="ts">The timestamp of the document.</param>
    public NamedDocument(string name, Module coll, DateTime ts) : base(coll, ts)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the NamedDocument class with the specified name, coll, ts, and additional data stored
    /// as key/value pairs on the instance.
    /// </summary>
    /// <param name="name">The string value of the document name.</param>
    /// <param name="coll">The module to which the document belongs.</param>
    /// <param name="ts">The timestamp of the document.</param>
    /// <param name="data">Additional data on the document.</param>
    public NamedDocument(string name, Module coll, DateTime ts, IDictionary<string, object?> data) : base(coll, ts, data)
    {
        Name = name;
    }
}
