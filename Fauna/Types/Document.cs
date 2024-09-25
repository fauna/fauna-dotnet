namespace Fauna.Types;

/// <summary>
/// Represents a document.
/// </summary>
public sealed class Document : BaseDocument
{

    /// <summary>
    /// Gets the string value of the document id.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Initializes a new instance of the Document class with the specified id, coll, and ts.
    /// </summary>
    /// <param name="id">The string value of the document id.</param>
    /// <param name="coll">The module to which the document belongs.</param>
    /// <param name="ts">The timestamp of the document.</param>
    public Document(string id, Module coll, DateTime ts) : base(coll, ts)
    {
        Id = id;
    }

    /// <summary>
    /// Initializes a new instance of the Document class with the specified id, coll, ts, and additional data stored
    /// as key/value pairs on the instance.
    /// </summary>
    /// <param name="id">The string value of the document id.</param>
    /// <param name="coll">The module to which the document belongs.</param>
    /// <param name="ts">The timestamp of the document.</param>
    /// <param name="data">Additional data on the document.</param>
    public Document(string id, Module coll, DateTime ts, IDictionary<string, object?> data) : base(coll, ts, data)
    {
        Id = id;
    }
}
