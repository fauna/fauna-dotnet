namespace Fauna;

public class FaunaWriteConstraintException : FaunaQueryRuntimeException
{
    public FaunaWriteConstraintException(string message, QueryFailure? queryFailure = null)
        : base(message, queryFailure) { }

    public FaunaWriteConstraintException(string message, Exception innerException, QueryFailure? queryFailure = null)
        : base(message, innerException, queryFailure) { }
}
