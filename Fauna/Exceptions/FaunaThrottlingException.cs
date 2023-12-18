namespace Fauna;

/// <summary>
/// Represents an exception that indicates some capacity limit was exceeded and thus the request could not be served.
/// This exception is considered retryable after a suitable delay.
/// </summary>
public class FaunaThrottlingException : FaunaServiceException, IFaunaRetryableException
{
    public FaunaThrottlingException(string message, QueryFailure? queryFailure = null)
        : base(message, queryFailure) { }

    public FaunaThrottlingException(string message, Exception innerException, QueryFailure? queryFailure = null)
        : base(message, innerException, queryFailure) { }
}
