using System.Net;
using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception that indicates some capacity limit was exceeded and thus the request could not be served.
/// This exception is considered retryable after a suitable delay.
/// </summary>
public class ThrottlingException : ServiceException, IRetryableException
{

    /// <summary>
    /// Initializes a new instance of the <see cref="ThrottlingException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ThrottlingException(string message) : base(message)
    {
        StatusCode = HttpStatusCode.TooManyRequests;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThrottlingException"/> class with a specified error message and query failure details.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="failure">A <see cref="QueryFailure"/></param>
    public ThrottlingException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
