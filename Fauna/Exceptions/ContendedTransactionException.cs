namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception that occurs when a transaction is aborted due to concurrent modification.
/// This exception is considered retryable after a suitable delay.
/// </summary>
public class ContendedTransactionException : ServiceException, IRetryableException
{
    public ContendedTransactionException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }
}
