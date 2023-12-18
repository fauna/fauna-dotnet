namespace Fauna;

/// <summary>
/// Represents exceptions caused by invalid requests to Fauna.
/// </summary>
public class FaunaInvalidRequestException : FaunaServiceException
{
    public FaunaInvalidRequestException(string message, QueryFailure? queryFailure = null)
        : base(message, queryFailure) { }

    public FaunaInvalidRequestException(string message, Exception innerException, QueryFailure? queryFailure = null)
        : base(message, innerException, queryFailure) { }
}
