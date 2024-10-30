using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents exceptions thrown when the query execution time exceeds the specified or default timeout period.
/// </summary>
public class QueryTimeoutException : TimeoutException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryTimeoutException"/> class with a specified error message and query failure details.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="failure">A <see cref="QueryFailure"/>.</param>
    public QueryTimeoutException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
