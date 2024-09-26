using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents exceptions thrown when the query fails at runtime.
/// </summary>
public class QueryRuntimeException : ServiceException
{
    public QueryRuntimeException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
