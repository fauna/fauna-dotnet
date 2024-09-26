using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents exceptions thrown when the query execution time exceeds the specified or default timeout period.
/// </summary>
public class QueryTimeoutException : TimeoutException
{
    public QueryTimeoutException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
