namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception thrown when there is an authentication error in Fauna.
/// Corresponds to the 'unauthorized' error code in Fauna.
/// </summary>
public class AuthenticationException : ServiceException
{
    public AuthenticationException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }
}
