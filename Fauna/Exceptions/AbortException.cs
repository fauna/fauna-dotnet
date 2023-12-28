using Fauna.Serialization;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception that occurs when the FQL `abort` function is called.
/// This exception captures the data provided during the abort operation.
/// </summary>
public class AbortException : QueryRuntimeException
{
    private readonly Dictionary<Type, object?> cache = new();
    private static readonly Type NonTypedKey = typeof(object);

    /// <summary>
    /// Initializes a new instance of the <see cref="AbortException"/> class with a specified error message and query failure details.
    /// </summary>
    /// <param name="queryFailure">The <see cref="QueryFailure"/> object containing details about the query failure.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public AbortException(QueryFailure queryFailure, string message)
        : base(queryFailure, message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AbortException"/> class with a specified error message, a reference to the inner exception, and query failure details.
    /// </summary>
    /// <param name="queryFailure">The <see cref="QueryFailure"/> object containing details about the query failure.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public AbortException(QueryFailure queryFailure, string message, Exception innerException)
        : base(queryFailure, message, innerException) { }

    /// <summary>
    /// Retrieves the deserialized data associated with the abort operation as an object.
    /// </summary>
    /// <returns>The deserialized data as an object, or null if no data is available.</returns>
    public object? GetData()
    {
        if (!cache.TryGetValue(NonTypedKey, out var cachedData))
        {
            var abortDataString = QueryFailure.ErrorInfo.Abort?.ToString();
            if (!string.IsNullOrEmpty(abortDataString))
            {
                cachedData = Serializer.Deserialize(abortDataString);
                cache[NonTypedKey] = cachedData;
            }
        }
        return cachedData;
    }

    /// <summary>
    /// Retrieves the deserialized data associated with the abort operation as a specific type.
    /// </summary>
    /// <typeparam name="T">The type to which the data should be deserialized.</typeparam>
    /// <returns>The deserialized data as the specified type, or null if no data is available.</returns>
    public T? GetData<T>()
    {
        var typeKey = typeof(T);
        if (!cache.TryGetValue(typeKey, out var cachedData))
        {
            var abortDataString = QueryFailure.ErrorInfo.Abort.ToString();
            if (!string.IsNullOrEmpty(abortDataString))
            {
                T? deserializedResult = Serializer.Deserialize<T>(abortDataString);
                cache[typeKey] = deserializedResult;
                return deserializedResult;
            }
        }
        return (T?)cachedData;
    }
}
