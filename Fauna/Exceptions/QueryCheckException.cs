namespace Fauna.Exceptions;

public class QueryCheckException : ServiceException
{
    public QueryCheckException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }

    public QueryCheckException(QueryFailure queryFailure, string message, Exception innerException)
        : base(queryFailure, message, innerException) { }
}
