namespace Fauna;

public class FaunaServiceException : FaunaBaseException
{
    /// <summary>
    /// Gets the QueryFailure object associated with the exception.
    /// It provides information about the failure in the context of a Fauna query.
    /// </summary>
    public QueryFailure? QueryFailure { get; }

    public FaunaServiceException(string message, QueryFailure queryFailure)
        : base(message)
    {
        QueryFailure = queryFailure;
    }

    public FaunaServiceException(string message, Exception innerException, QueryFailure queryFailure)
        : base(message, innerException)
    {
        QueryFailure = queryFailure;
    }
}
