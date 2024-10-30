using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception thrown when access to a resource is not allowed.
/// Corresponds to the 'forbidden' error code in Fauna.
/// </summary>
public class AuthorizationException : ServiceException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationException"/> class with a specified error message and query failure details.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="failure">A <see cref="QueryFailure"/></param>
    public AuthorizationException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
