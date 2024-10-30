using Fauna.Core;
using Fauna.Mapping;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception that occurs when constraints are violated in a query.
/// This exception captures the specific constraint failures for inspection.
/// </summary>
public class ConstraintFailureException : ServiceException
{
    /// <summary>
    /// The constraint failures related to the exception.
    /// </summary>
    public ConstraintFailure[]? ConstraintFailures { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstraintFailureException"/> class with a specified error message and query failure details.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="failure">A <see cref="QueryFailure"/></param>
    public ConstraintFailureException(string message, QueryFailure failure)
        : base(message, failure)
    {
        ConstraintFailures = failure.ConstraintFailures;
    }
}
