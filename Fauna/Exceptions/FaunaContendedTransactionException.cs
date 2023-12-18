namespace Fauna;

/// <summary>
/// Represents an exception that occurs when a transaction is aborted due to concurrent modification.
/// This exception is considered retryable after a suitable delay.
/// </summary>
public class FaunaContendedTransactionException : FaunaServiceException, IFaunaRetryableException
{
    public FaunaContendedTransactionException(string message, QueryFailure? queryFailure = null)
        : base(message, queryFailure) { }

    public FaunaContendedTransactionException(string message, Exception innerException, QueryFailure? queryFailure = null)
        : base(message, innerException, queryFailure) { }
}
