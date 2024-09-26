using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception thrown when there is an authorization error in Fauna.
/// Corresponds to the 'unauthorized' error code in Fauna.
/// </summary>
public class AuthenticationException : ServiceException
{
    public AuthenticationException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
