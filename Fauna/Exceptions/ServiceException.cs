namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception related to Fauna service errors, particularly for query failures.
/// </summary>
public class ServiceException : FaunaException
{
    /// <summary>
    /// Gets the <see cref="QueryFailure"/> object associated with the exception.
    /// This object provides detailed information about the failure in the context of a Fauna query.
    /// </summary>
    public QueryFailure QueryFailure { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceException"/> class with a specified query failure details and error message.
    /// </summary>
    /// <param name="queryFailure">The <see cref="QueryFailure"/> object containing details about the query failure.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ServiceException(QueryFailure queryFailure, string message)
        : base(message) => QueryFailure = queryFailure;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceException"/> class with a specified query failure details, error message, and a reference to the inner exception.
    /// </summary>
    /// <param name="queryFailure">The <see cref="QueryFailure"/> object containing details about the query failure.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, represented by an <see cref="Exception"/> object.</param>
    public ServiceException(QueryFailure queryFailure, string message, Exception innerException)
        : base(message, innerException) => QueryFailure = queryFailure;
}
