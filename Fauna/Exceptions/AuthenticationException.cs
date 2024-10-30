using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception thrown when there is an authorization error in Fauna.
/// Corresponds to the 'unauthorized' error code in Fauna.
/// </summary>
public class AuthenticationException : ServiceException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationException"/> class with a specified error message and query failure details.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="failure">A <see cref="QueryFailure"/></param>
    public AuthenticationException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
