using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception that occurs when a transaction is aborted due to concurrent modification.
/// This exception is considered retryable after a suitable delay.
/// </summary>
public class ContendedTransactionException : ServiceException, IRetryableException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContendedTransactionException"/> class with a specified error message and query failure details.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="failure">A <see cref="QueryFailure"/></param>
    public ContendedTransactionException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
