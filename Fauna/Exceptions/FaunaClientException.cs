namespace Fauna;

/// <summary>
/// Represents exception encountered prior to sending the request to Fauna.
/// </summary>
public class FaunaClientException : FaunaBaseException
{
    public FaunaClientException(string message, QueryFailure? queryFailure = null)
        : base(message, queryFailure) { }

    public FaunaClientException(string message, Exception innerException, QueryFailure? queryFailure = null)
        : base(message, innerException, queryFailure) { }
}
