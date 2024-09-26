using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents exceptions thrown when the query execution time exceeds the specified or default timeout period.
/// </summary>
public class TimeoutException : ServiceException
{
    public TimeoutException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
