using Fauna.Types;

namespace Fauna.Exceptions;

internal class NullDocumentException : Exception
{
    public string? Id { get; }
    public string? Name { get; }
    public Module Collection { get; }
    public string Cause { get; }


    public NullDocumentException(string? id, string? name, Module collection, string cause) : base($"Document {id ?? name} in collection {collection.Name} is null: {cause}")
    {
        Id = id;
        Name = name;
        Collection = collection;
        Cause = cause;
    }
}
