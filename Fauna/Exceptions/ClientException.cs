namespace Fauna.Exceptions;

/// <summary>
/// Represents exception encountered prior to sending the request to Fauna.
/// </summary>
public class ClientException : FaunaException
{
    public ClientException(string message)
        : base(message) { }

    public ClientException(string message, Exception innerException)
        : base(message, innerException) { }
}
