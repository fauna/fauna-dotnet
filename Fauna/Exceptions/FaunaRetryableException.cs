namespace Fauna;

/// <summary>
/// Represents exceptions that are potentially recoverable through retrying the failed operation.
/// </summary>
public class FaunaRetryableException : FaunaBaseException, IFaunaRetryableException
{
    public FaunaRetryableException(string message, QueryFailure? queryFailure = null)
        : base(message, queryFailure) { }

    public FaunaRetryableException(string message, Exception innerException, QueryFailure? queryFailure = null)
        : base(message, innerException, queryFailure) { }
}
