namespace Fauna;

/// <summary>
/// Represents exception encountered prior to sending the request to Fauna.
/// </summary>
public class FaunaClientException : FaunaBaseException
{
    public FaunaClientException(string message)
        : base(message) { }

    public FaunaClientException(string message, Exception innerException)
        : base(message, innerException) { }
}
