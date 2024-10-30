namespace Fauna.Exceptions;

/// <summary>
/// Represents error that occur during serialization and deserialization of Fauna data.
/// </summary>
public class SerializationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public SerializationException(string? message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The inner exception.</param>
    public SerializationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
