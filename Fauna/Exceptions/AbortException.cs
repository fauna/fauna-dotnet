using Fauna.Core;
using Fauna.Mapping;
using Fauna.Serialization;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception that occurs when the FQL `abort` function is called.
/// This exception captures the data provided during the abort operation.
/// </summary>
public class AbortException : ServiceException
{
    private readonly MappingContext _ctx;
    private readonly Dictionary<Type, object?> _cache = new();
    private readonly object? _abortRaw;


    /// <summary>
    /// Initializes a new instance of the <see cref="AbortException"/> class with a specified error message and query failure details.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="failure">A <see cref="QueryFailure"/></param>
    /// <param name="ctx">A mapping context.</param>
    public AbortException(string message, QueryFailure failure, MappingContext ctx)
        : base(message, failure)
    {
        _ctx = ctx;
        _abortRaw = failure.Abort;
    }

    /// <summary>
    /// Retrieves the deserialized data associated with the abort operation as an object.
    /// </summary>
    /// <returns>The deserialized data as an object, or null if no data is available.</returns>
    public object? GetData() => GetData(Serializer.Dynamic);

    /// <summary>
    /// Retrieves the deserialized data associated with the abort operation as a specific type.
    /// </summary>
    /// <typeparam name="T">The type to which the data should be deserialized.</typeparam>
    /// <returns>The deserialized data as the specified type, or null if no data is available.</returns>
    public T? GetData<T>() where T : notnull => GetData(Serializer.Generate<T>(_ctx));

    /// <summary>
    /// Retrieves the deserialized data associated with the abort operation as a specific type.
    /// </summary>
    /// <typeparam name="T">The type to which the data should be deserialized.</typeparam>
    /// <param name="serializer">A serializer for the abort data.</param>
    /// <returns>The deserialized data as the specified type, or null if no data is available.</returns>
    public T? GetData<T>(ISerializer<T> serializer)
    {
        var typeKey = typeof(T);
        if (_cache.TryGetValue(typeKey, out var cachedData)) return (T?)cachedData;

        if (_abortRaw == null) return (T?)cachedData;

        var abortDataString = _abortRaw.ToString();
        if (string.IsNullOrEmpty(abortDataString)) return (T?)cachedData;

        // TODO(matt) pull from context
        var reader = new Utf8FaunaReader(abortDataString);
        reader.Read();

        var deserializedResult = serializer.Deserialize(_ctx, ref reader);
        _cache[typeKey] = deserializedResult;
        return deserializedResult;
    }
}
