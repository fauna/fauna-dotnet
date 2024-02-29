using Fauna.Types;

namespace Fauna.Exceptions;

internal class NullDocumentException : Exception
{
    public string Id { get; }

    public Module Collection { get; }

    public string Cause { get; }

    public NullDocumentException(string id, Module collection, string cause) : base($"Document {id} in collection {collection.Name} is null: {cause}")
    {
        Id = id;
        Collection = collection;
        Cause = cause;
    }
}
