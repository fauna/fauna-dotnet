namespace Fauna.Exceptions;

public class QueryTimeoutException : ServiceException
{
    public QueryTimeoutException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }

    public QueryTimeoutException(QueryFailure queryFailure, string message, Exception innerException)
        : base(queryFailure, message, innerException) { }
}
