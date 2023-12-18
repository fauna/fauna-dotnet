namespace Fauna;

public class FaunaQueryCheckException : FaunaServiceException
{
    public FaunaQueryCheckException(string message, QueryFailure? queryFailure = null)
        : base(message, queryFailure) { }

    public FaunaQueryCheckException(string message, Exception innerException, QueryFailure? queryFailure = null)
        : base(message, innerException, queryFailure) { }
}
