using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents exceptions caused by invalid requests to Fauna.
/// </summary>
public class InvalidRequestException : ServiceException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidRequestException"/> class with a specified error message and query failure details.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="failure">A <see cref="QueryFailure"/></param>
    public InvalidRequestException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
