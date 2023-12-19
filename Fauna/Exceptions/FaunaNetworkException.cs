namespace Fauna;

/// <summary>
/// Represents network-related exceptions thrown inside Connection, indicating no request was made.
/// </summary>
public class FaunaNetworkException : FaunaBaseException, IFaunaRetryableException
{
    public FaunaNetworkException(string message, QueryFailure? queryFailure = null)
        : base(message, queryFailure) { }

    public FaunaNetworkException(string message, Exception innerException, QueryFailure? queryFailure = null)
        : base(message, innerException, queryFailure) { }
}
