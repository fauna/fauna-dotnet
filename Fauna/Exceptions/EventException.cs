using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception related to Fauna event stream and event feed errors.
/// </summary>
public class EventException : ServiceException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventException"/> class.
    /// </summary>
    /// <param name="err">The <see cref="ErrorInfo"/> from which to extract a message.</param>
    public EventException(ErrorInfo err) : base(message: err.Message!) { }
}
