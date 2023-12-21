namespace Fauna.Exceptions;

public class AuthenticationException : ServiceException
{
    public AuthenticationException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }

    public AuthenticationException(QueryFailure queryFailure, string message, Exception innerException)
        : base(queryFailure, message, innerException) { }
}