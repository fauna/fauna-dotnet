namespace Fauna;

/// <summary>
/// An error response that is the result of the query failing during execution. QueryRuntimeError's occur when a bug in your query causes an invalid execution to be requested.
/// </summary>
public class FaunaQueryRuntimeException : FaunaServiceException
{
    public FaunaQueryRuntimeException(string message, QueryFailure? queryFailure = null)
        : base(message, queryFailure) { }

    public FaunaQueryRuntimeException(string message, Exception innerException, QueryFailure? queryFailure = null)
        : base(message, innerException, queryFailure) { }
}
