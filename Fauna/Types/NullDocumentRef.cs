namespace Fauna.Types;

public class NullDocumentRef : DocumentRef
{
    /// <summary>
    /// Gets or sets the cause that the document is null.
    /// </summary>
    /// <value>
    /// A string representing the cause that the document is null.
    /// </value>
    public string? Cause { get; set; } = null;
}
