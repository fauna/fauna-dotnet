using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception thrown for a bad gateway.
/// Corresponds to the 'bad_gateway' error code in Fauna.
/// </summary>
public class BadGatewayException : ServiceException
{
    public BadGatewayException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}
