using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents exceptions thrown when the query has syntax errors.
/// </summary>
public class QueryCheckException : ServiceException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryCheckException"/> class with a specified error message and query failure details.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="failure">A <see cref="QueryFailure"/>.</param>
    public QueryCheckException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
