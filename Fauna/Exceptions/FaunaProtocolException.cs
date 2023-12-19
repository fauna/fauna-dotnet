namespace Fauna;

/// <summary>
/// Represents exceptions when a response does not match the wire protocol.
/// </summary>
public class FaunaProtocolException : FaunaBaseException
{
    public FaunaProtocolException(string message)
        : base(message) { }

    public FaunaProtocolException(string message, Exception innerException)
        : base(message, innerException) { }
}
