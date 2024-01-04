namespace Fauna.Exceptions;

/// <summary>
/// Represents exceptions caused by invalid requests to Fauna.
/// </summary>
public class InvalidRequestException : ServiceException
{
    public InvalidRequestException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }
}
