using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents the base exception class for all exceptions specific to Fauna interactions.
/// </summary>
public class FaunaException : Exception
{
    public FaunaException() { }

    public FaunaException(string message) : base(message) { }

    public FaunaException(string message, Exception innerException)
    : base(message, innerException) { }

    public FaunaException(ErrorInfo err) : base(message: err.Message) { }
}

