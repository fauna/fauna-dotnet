namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception thrown when there is an authorization error in Fauna.
/// Corresponds to the 'forbidden' error code in Fauna.
/// </summary>
public class AuthorizationException : ServiceException
{
    public AuthorizationException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }
}
