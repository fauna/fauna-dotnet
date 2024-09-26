using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents exceptions caused by invalid requests to Fauna.
/// </summary>
public class InvalidRequestException : ServiceException
{
    public InvalidRequestException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
