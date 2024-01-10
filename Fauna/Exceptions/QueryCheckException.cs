namespace Fauna.Exceptions;

/// <summary>
/// Represents exceptions thrown when the query has syntax errors.
/// </summary>
public class QueryCheckException : ServiceException
{
    public QueryCheckException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }
}
