namespace Fauna.Exceptions;

/// <summary>
/// Represents network-related exceptions thrown inside Connection, indicating no request was made.
/// </summary>
public class NetworkException : FaunaException, IRetryableException
{
    public NetworkException(string message)
        : base(message) { }

    public NetworkException(string message, Exception innerException)
        : base(message, innerException) { }
}
