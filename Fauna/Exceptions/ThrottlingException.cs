using System.Net;
using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception that indicates some capacity limit was exceeded and thus the request could not be served.
/// This exception is considered retryable after a suitable delay.
/// </summary>
public class ThrottlingException : ServiceException, IRetryableException
{
    public ThrottlingException(string message) : base(message)
    {
        StatusCode = HttpStatusCode.TooManyRequests;
    }
    public ThrottlingException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
