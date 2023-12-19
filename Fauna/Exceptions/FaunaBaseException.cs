namespace Fauna;

/// <summary>
/// Represents the base exception class for all exceptions specific to Fauna interactions.
/// </summary>
public class FaunaBaseException : Exception
{
    public FaunaBaseException() { }

    public FaunaBaseException(string message) : base(message) { }

    public FaunaBaseException(string message, Exception innerException)
    : base(message, innerException) { }
}
