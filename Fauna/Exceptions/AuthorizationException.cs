using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception thrown when access to a resource is not allowed.
/// Corresponds to the 'forbidden' error code in Fauna.
/// </summary>
public class AuthorizationException : ServiceException
{
    public AuthorizationException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
