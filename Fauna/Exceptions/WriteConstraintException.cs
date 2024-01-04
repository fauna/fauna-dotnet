namespace Fauna.Exceptions;

public class WriteConstraintException : QueryRuntimeException
{
    public WriteConstraintException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }
}
