namespace Fauna;

/// <summary>
/// Represents network-related exceptions thrown inside Connection, indicating no request was made.
/// </summary>
public class FaunaNetworkException : FaunaBaseException, IFaunaRetryableException
{
    public FaunaNetworkException(string message)
        : base(message) { }

    public FaunaNetworkException(string message, Exception innerException)
        : base(message, innerException) { }
}
