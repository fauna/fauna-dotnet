namespace Fauna;

/// <summary>
/// Represents an exception related to Fauna service errors, particularly for query failures.
/// </summary>
public class FaunaServiceException : FaunaBaseException
{
    /// <summary>
    /// Gets the <see cref="QueryFailure"/> object associated with the exception.
    /// This object provides detailed information about the failure in the context of a Fauna query.
    /// </summary>
    public QueryFailure QueryFailure { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FaunaServiceException"/> class with a specified error message and query failure details.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="queryFailure">The <see cref="QueryFailure"/> object containing details about the query failure.</param>
    public FaunaServiceException(string message, QueryFailure queryFailure)
        : base(message)
    {
        QueryFailure = queryFailure;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FaunaServiceException"/> class with a specified error message, a reference to the inner exception, and query failure details.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, represented by an <see cref="Exception"/> object.</param>
    /// <param name="queryFailure">The <see cref="QueryFailure"/> object containing details about the query failure.</param>
    public FaunaServiceException(string message, Exception innerException, QueryFailure queryFailure)
        : base(message, innerException)
    {
        QueryFailure = queryFailure;
    }
}
