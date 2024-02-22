using Fauna.Types;

namespace Fauna.Exceptions;

internal class NullDocumentException : Exception
{
    public string Id { get; }

    public Module Collection { get; }

    public string Cause { get; }

    public NullDocumentException(string message, string id, Module collection, string cause) : base(message)
    {
        Id = id;
        Collection = collection;
        Cause = cause;
    }
}
