namespace Fauna.Types;

public class NullDocumentRef : DocumentRef
{
    /// <summary>
    /// Gets or sets the reason that the document is null.
    /// </summary>
    /// <value>
    /// A string representing the reason that the document is null.
    /// </value>
    public string? Reason { get; set; } = null;
}