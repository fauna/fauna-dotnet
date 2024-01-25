namespace Fauna.Types;

/// <summary>
/// Represents a null reference to a document, including a reason for its null state.
/// </summary>
public class NullDocumentRef : DocumentRef
{
    public NullDocumentRef(string id, Module collection, string cause) : base(id, collection)
    {
        Cause = cause;
    }
    /// <summary>
    /// Gets or sets the cause that the document is null.
    /// </summary>
    /// <value>
    /// A string representing the cause that the document is null.
    /// </value>
    public string Cause { get; }
}
