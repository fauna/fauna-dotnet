using Fauna.Types;

namespace Fauna.Exceptions;

/// <summary>
/// An exception representing a case when a document cannot be materialized because it does not exist.
/// </summary>
public class NullDocumentException : Exception
{
    /// <summary>
    /// The ID associated with the document. In the case of named documents, this will be null.
    /// </summary>
    public string? Id { get; }

    /// <summary>
    /// The name associated with the document. In the case of user documents, this will be null.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// The collection to which the document belongs.
    /// </summary>
    public Module Collection { get; }

    /// <summary>
    /// The cause for the null document.
    /// </summary>
    public string Cause { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NullDocumentException"/> class.
    /// </summary>
    /// <param name="id">The ID of the document. Should be null if it's a named document.</param>
    /// <param name="name">The name of the document. Should be null if it's a user docuemnt.</param>
    /// <param name="collection">The collection associated with the document.</param>
    /// <param name="cause">The cause of the null document.</param>
    public NullDocumentException(string? id, string? name, Module collection, string cause) : base($"Document {id ?? name} in collection {collection.Name} is null: {cause}")
    {
        Id = id;
        Name = name;
        Collection = collection;
        Cause = cause;
    }
}
