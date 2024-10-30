using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents exceptions thrown when the query fails at runtime.
/// </summary>
public class QueryRuntimeException : ServiceException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryRuntimeException"/> class with a specified error message and query failure details.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="failure">A <see cref="QueryFailure"/>.</param>
    public QueryRuntimeException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
