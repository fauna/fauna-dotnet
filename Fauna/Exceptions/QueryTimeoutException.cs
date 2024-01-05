namespace Fauna.Exceptions;

public class QueryTimeoutException : ServiceException
{
    public QueryTimeoutException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }
}
