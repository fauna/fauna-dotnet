namespace Fauna.Exceptions;

/// <summary>
/// Represents error that occur during serialization and deserialization of Fauna data.
/// </summary>
public class SerializationException : Exception
{
    public SerializationException(string? message) : base(message)
    {
    }

    public SerializationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
