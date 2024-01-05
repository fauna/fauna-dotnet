namespace Fauna.Exceptions;

/// <summary>
/// An error response that is the result of the query failing during execution. QueryRuntimeError's occur when a bug in your query causes an invalid execution to be requested.
/// </summary>
public class QueryRuntimeException : ServiceException
{
    public QueryRuntimeException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }
}
