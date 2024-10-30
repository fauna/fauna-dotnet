using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception thrown for a bad gateway.
/// Corresponds to the 'bad_gateway' error code in Fauna.
/// </summary>
public class BadGatewayException : ServiceException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BadGatewayException"/> class with a specified error message and query failure details.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="failure">A <see cref="QueryFailure"/></param>
    public BadGatewayException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
