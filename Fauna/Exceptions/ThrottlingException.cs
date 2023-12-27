namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception that indicates some capacity limit was exceeded and thus the request could not be served.
/// This exception is considered retryable after a suitable delay.
/// </summary>
public class ThrottlingException : ServiceException, IRetryableException
{
    public ThrottlingException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }

    public ThrottlingException(QueryFailure queryFailure, string message, Exception innerException)
        : base(queryFailure, message, innerException) { }
}
