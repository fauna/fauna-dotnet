namespace Fauna.Exceptions;

public class WriteConstraintException : QueryRuntimeException
{
    public WriteConstraintException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }

    public WriteConstraintException(QueryFailure queryFailure, string message, Exception innerException)
        : base(queryFailure, message, innerException) { }
}
