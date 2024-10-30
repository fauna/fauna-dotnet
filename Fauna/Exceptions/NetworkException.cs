using System.Net;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception that occurs when a request fails due to a network issue.
/// </summary>
public class NetworkException : FaunaException
{
    /// <summary>
    /// The response body that caused the <see cref="ProtocolException"/> to be thrown.
    /// </summary>
    public string ResponseBody { get; init; }

    /// <summary>
    /// The HTTP status code associated with the <see cref="ProtocolException"/>.
    /// </summary>
    public HttpStatusCode StatusCode { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="statusCode">The HTTP status code from the related HTTP request.</param>
    /// <param name="body">The HTTP response body that was out of protocol.</param>
    public NetworkException(string message, HttpStatusCode statusCode, string body)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = body;
    }
}
