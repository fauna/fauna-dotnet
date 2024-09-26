using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents exceptions thrown when the query has syntax errors.
/// </summary>
public class QueryCheckException : ServiceException
{
    public QueryCheckException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
