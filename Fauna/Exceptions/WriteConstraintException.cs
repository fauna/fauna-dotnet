namespace Fauna.Exceptions;

/// <summary>
/// Represents exceptions thrown due to constraint violations on write operations.
/// </summary>
public class WriteConstraintException : QueryRuntimeException
{
    public WriteConstraintException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }
}
