namespace Fauna;

public class FaunaQueryTimeoutException : FaunaServiceException
{
    public FaunaQueryTimeoutException(string message, QueryFailure? queryFailure = null)
        : base(message, queryFailure) { }

    public FaunaQueryTimeoutException(string message, Exception innerException, QueryFailure? queryFailure = null)
        : base(message, innerException, queryFailure) { }
}
