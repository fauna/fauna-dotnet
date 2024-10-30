using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents the base exception class for all exceptions specific to Fauna interactions.
/// </summary>
public class FaunaException : Exception
{
    /// <summary>
    /// Initializes a FaunaException with a message.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public FaunaException(string message) : base(message) { }

    /// <summary>
    /// Initializes a FaunaException with a message and inner exception.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public FaunaException(string message, Exception innerException)
    : base(message, innerException) { }

    /// <summary>
    /// Initializes a FaunaException from an <see cref="ErrorInfo"/> instance.
    /// </summary>
    /// <param name="err">The <see cref="ErrorInfo"/> from which to extract a message.</param>
    public FaunaException(ErrorInfo err) : base(message: err.Message) { }
}

