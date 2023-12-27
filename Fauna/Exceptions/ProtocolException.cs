namespace Fauna.Exceptions;

/// <summary>
/// Represents exceptions when a response does not match the wire protocol.
/// </summary>
public class ProtocolException : FaunaException
{
    public ProtocolException(string message)
        : base(message) { }

    public ProtocolException(string message, Exception innerException)
        : base(message, innerException) { }
}
