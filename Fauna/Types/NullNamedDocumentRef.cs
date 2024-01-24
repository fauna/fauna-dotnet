namespace Fauna.Types;

/// <summary>
/// Represents a reference to a named document that is null, including a reason for its null state.
/// This class extends NamedDocumentRef to provide additional context for null references in the database.
/// </summary>
public class NullNamedDocumentRef : NamedDocumentRef
{
    public NullNamedDocumentRef(string name, Module collection, string cause) : base(name, collection)
    {
        Cause = cause;
    }

    /// <summary>
    /// Gets or sets the cause that the document is null.
    /// </summary>
    /// <value>
    /// A string representing the cause that the document is null.
    /// </value>
    public string Cause { get; set; }
}
