namespace Fauna;

/// <summary>
/// Represents exceptions when a response does not match the wire protocol.
/// </summary>
public class FaunaProtocolException : FaunaBaseException
{
    public FaunaProtocolException(string message, QueryFailure? queryFailure = null)
        : base(message, queryFailure) { }

    public FaunaProtocolException(string message, Exception innerException, QueryFailure? queryFailure = null)
        : base(message, innerException, queryFailure) { }
}
