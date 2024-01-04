namespace Fauna.Exceptions;

public class QueryCheckException : ServiceException
{
    public QueryCheckException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }
}
