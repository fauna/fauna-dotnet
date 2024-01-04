namespace Fauna.Exceptions;

public class AuthenticationException : ServiceException
{
    public AuthenticationException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }
}
