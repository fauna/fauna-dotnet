namespace Fauna.Exceptions;

/// <summary>
/// Represents exceptions thrown when the query execution time exceeds the specified or default timeout period.
/// </summary>
public class QueryTimeoutException : ServiceException
{
    public QueryTimeoutException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }
}
