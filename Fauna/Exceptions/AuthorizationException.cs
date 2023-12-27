namespace Fauna.Exceptions;

public class AuthorizationException : ServiceException
{
    public AuthorizationException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }

    public AuthorizationException(QueryFailure queryFailure, string message, Exception innerException)
        : base(queryFailure, message, innerException) { }
}