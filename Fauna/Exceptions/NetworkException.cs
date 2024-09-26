using System.Net;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception that occurs when a request fails due to a network issue.
/// </summary>
public class NetworkException : FaunaException
{
    public string ResponseBody { get; init; }

    public HttpStatusCode StatusCode { get; init; }

    public NetworkException(string message, HttpStatusCode statusCode, string body)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = body;
    }
}
