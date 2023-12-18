namespace Fauna;

/// <summary>
/// Represents the base exception class for all exceptions specific to Fauna interactions.
/// </summary>
public class FaunaBaseException : Exception
{
    /// <summary>
    /// Gets the QueryFailure object associated with the exception, if any.
    /// It provides information about the failure in the context of a Fauna query.
    /// </summary>
    public QueryFailure? QueryFailure { get; }

    public FaunaBaseException() { }

    public FaunaBaseException(string message) : base(message) { }

    public FaunaBaseException(string message, QueryFailure? queryFailure = null)
    : base(message)
    {
        QueryFailure = queryFailure;
    }

    public FaunaBaseException(string message, Exception innerException, QueryFailure? queryFailure = null)
    : base(message, innerException)
    {
        QueryFailure = queryFailure;
    }
}
