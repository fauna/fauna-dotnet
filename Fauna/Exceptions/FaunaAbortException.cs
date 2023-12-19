using Fauna.Serialization;

namespace Fauna;

/// <summary>
/// Represents an exception that occurs when the FQL `abort` function is called.
/// This exception captures the data provided during the abort operation.
/// </summary>
public class FaunaAbortException : FaunaQueryRuntimeException
{
    private readonly Lazy<object?> deserializedData;

    /// <summary>
    /// Initializes a new instance of the FaunaAbortException class with a specified error message, a reference to the inner exception, and query failure details.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="queryFailure">The QueryFailure object containing details about the query failure.</param>
    public FaunaAbortException(string message, QueryFailure queryFailure)
        : base(message, queryFailure) => deserializedData = new(DeserializeAbortData);

    /// <summary>
    /// Initializes a new instance of the FaunaAbortException class with a specified error message, a reference to the inner exception, and query failure details.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="queryFailure">The QueryFailure object containing details about the query failure.</param>
    public FaunaAbortException(string message, Exception innerException, QueryFailure queryFailure)
        : base(message, innerException, queryFailure) => deserializedData = new(DeserializeAbortData);

    /// <summary>
    /// Retrieves the deserialized abort data as an object.
    /// </summary>
    /// <returns>The deserialized abort data, or null if the data is not available.</returns>
    public object? GetData() => deserializedData.Value;

    /// <summary>
    /// Retrieves the deserialized abort data as a specific type.
    /// </summary>
    /// <typeparam name="T">The type to which the data should be deserialized.</typeparam>
    /// <returns>The deserialized abort data as type T, or default if the data is not available or cannot be cast to type T.</returns>
    public T? GetData<T>() => deserializedData.Value is T data ? data : default;

    private object? DeserializeAbortData()
    {
        var abortDataString = QueryFailure?.ErrorInfo.Abort?.ToString();
        return !string.IsNullOrEmpty(abortDataString) ? Serializer.Deserialize(abortDataString) : null;
    }
}
