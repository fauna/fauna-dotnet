using System.Net;

namespace Fauna.Exceptions;

/// <summary>
/// Represents exceptions when a response does not match the wire protocol.
/// </summary>
public class ProtocolException : FaunaException
{
    public string ResponseBody { get; init; }

    public HttpStatusCode StatusCode { get; init; }

    public ProtocolException(string message, HttpStatusCode statusCode, string body)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = body;
    }
}
