using Fauna.Serialization;

namespace Fauna;

/// <summary>
/// Represents an exception due to calling the FQL `abort` function.
/// This exception captures the data provided during the abort operation.
/// </summary>
public class FaunaAbortException : FaunaQueryRuntimeException
{
    /// <summary>
    /// Gets the data associated with the abort operation.
    /// </summary>
    public object? AbortData { get; private set; }

    public FaunaAbortException(string message, QueryFailure queryFailure)
        : base(message, queryFailure)
    {
        InitializeAbortData(queryFailure);
    }

    public FaunaAbortException(string message, Exception innerException, QueryFailure queryFailure)
        : base(message, innerException, queryFailure)
    {
        InitializeAbortData(queryFailure);
    }

    private void InitializeAbortData(QueryFailure queryFailure)
    {
        if (queryFailure?.ErrorInfo.Abort is string abortDataString)
        {
            AbortData = Serializer.Deserialize(abortDataString);
        }
        else
        {
            AbortData = null;
        }
    }
}
