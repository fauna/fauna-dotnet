namespace Fauna;

public class FaunaServiceException : FaunaBaseException
{
    public FaunaServiceException(string message, QueryFailure? queryFailure = null)
        : base(message, queryFailure) { }

    public FaunaServiceException(string message, Exception innerException, QueryFailure? queryFailure = null)
        : base(message, innerException, queryFailure) { }
}
