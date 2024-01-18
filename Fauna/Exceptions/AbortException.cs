﻿using Fauna.Mapping;
using Fauna.Serialization;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception that occurs when the FQL `abort` function is called.
/// This exception captures the data provided during the abort operation.
/// </summary>
public class AbortException : QueryRuntimeException
{
    private readonly MappingContext _ctx;
    private readonly Dictionary<Type, object?> _cache = new();
    private static readonly Type NonTypedKey = typeof(object);

    /// <summary>
    /// Initializes a new instance of the <see cref="AbortException"/> class with a specified error message and query failure details.
    /// </summary>
    /// <param name="ctx">The <see cref="MappingContext"/></param>
    /// <param name="queryFailure">The <see cref="QueryFailure"/> object containing details about the query failure.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public AbortException(MappingContext ctx, QueryFailure queryFailure, string message)
        : base(queryFailure, message)
    {
        _ctx = ctx;
    }

    /// <summary>
    /// Retrieves the deserialized data associated with the abort operation as an object.
    /// </summary>
    /// <returns>The deserialized data as an object, or null if no data is available.</returns>
    public object? GetData() => GetData(Deserializer.Dynamic);

    /// <summary>
    /// Retrieves the deserialized data associated with the abort operation as a specific type.
    /// </summary>
    /// <typeparam name="T">The type to which the data should be deserialized.</typeparam>
    /// <param name="deserializer">A deserializer for the abort data.</param>
    /// <returns>The deserialized data as the specified type, or null if no data is available.</returns>
    public T? GetData<T>() where T : notnull => GetData(Deserializer.Generate<T>(_ctx));

    /// <summary>
    /// Retrieves the deserialized data associated with the abort operation as a specific type.
    /// </summary>
    /// <typeparam name="T">The type to which the data should be deserialized.</typeparam>
    /// <param name="deserializer">A deserializer for the abort data.</param>
    /// <returns>The deserialized data as the specified type, or null if no data is available.</returns>
    public T? GetData<T>(IDeserializer<T> deserializer)
    {
        var typeKey = typeof(T);
        if (!_cache.TryGetValue(typeKey, out var cachedData))
        {
            var abortDataString = QueryFailure.ErrorInfo.Abort?.ToString();
            if (!string.IsNullOrEmpty(abortDataString))
            {
                // TODO(matt) pull from context
                var reader = new Utf8FaunaReader(abortDataString);
                reader.Read();
                T? deserializedResult = deserializer.Deserialize(_ctx, ref reader);
                _cache[typeKey] = deserializedResult;
                return deserializedResult;
            }
        }
        return (T?)cachedData;
    }
}
