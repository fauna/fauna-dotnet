using System.Net;
using Fauna.Mapping;
using Fauna.Serialization;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an interface for exceptions that are potentially recoverable through retrying the failed operation.
/// </summary>
public interface IRetryableException { }

/// <summary>
/// Represents an exception related to Fauna service errors, particularly for query failures.
/// </summary>
public class ServiceException : FaunaException
{
    /// <summary>
    /// The HTTP status code.
    /// </summary>
    public HttpStatusCode? StatusCode { get; set; }

    /// <summary>
    /// The error code when a query fails.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// A comprehensive, human readable summary of any errors, warnings and/or logs returned from the query.
    /// </summary>
    public string? Summary { get; init; }

    /// <summary>
    /// The query stats for the request.
    /// </summary>
    public QueryStats Stats { get; init; }

    /// <summary>
    /// The transaction commit time in micros since epoch. Used by drivers to
    /// populate the x-last-txn-ts request header in order to get a consistent
    /// prefix RYOW guarantee.
    /// </summary>
    public long? TxnTs { get; init; }

    /// <summary>
    /// The schema version used by the query. This can be used by clients displaying
    /// schema to determine when they should refresh their schema. If the schema
    /// version that a client has stored differs from the one returned by the query,
    /// schema should be refreshed.
    /// </summary>
    public long? SchemaVersion { get; init; }

    /// <summary>
    /// The tags on the x-query-tags header, if it was provided.
    /// </summary>
    public IDictionary<string, string> QueryTags { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceException"/> class with a specified query failure details and error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ServiceException(string message)
        : base(message)
    {
        QueryTags = new Dictionary<string, string>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceException"/> class with a specified query failure details and error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="failure">A <see cref="QueryFailure"/></param>
    public ServiceException(string message, QueryFailure failure)
        : base(message)
    {
        StatusCode = failure.StatusCode;
        ErrorCode = failure.ErrorCode;
        Summary = failure.Summary;
        Stats = failure.Stats;
        TxnTs = failure.LastSeenTxn;
        SchemaVersion = failure.SchemaVersion;
        QueryTags = failure.QueryTags;
    }
}

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

/// <summary>
/// Represents an exception thrown when access to a resource is not allowed.
/// Corresponds to the 'forbidden' error code in Fauna.
/// </summary>
public class ForbiddenException : ServiceException
{
    public ForbiddenException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}

/// <summary>
/// Represents an exception thrown when there is an authorization error in Fauna.
/// Corresponds to the 'unauthorized' error code in Fauna.
/// </summary>
public class UnauthorizedException : ServiceException
{
    public UnauthorizedException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}

/// <summary>
/// Represents exceptions thrown when the query execution time exceeds the specified or default timeout period.
/// </summary>
public class TimeoutException : ServiceException
{
    public TimeoutException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}

/// <summary>
/// Represents exceptions thrown when the query execution time exceeds the specified or default timeout period.
/// </summary>
public class QueryTimeoutException : TimeoutException
{
    public QueryTimeoutException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}

/// <summary>
/// Represents exceptions thrown when the query has syntax errors.
/// </summary>
public class QueryCheckException : ServiceException
{
    public QueryCheckException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}

/// <summary>
/// Represents exceptions thrown when the query fails at runtime.
/// </summary>
public class QueryRuntimeException : ServiceException
{
    public QueryRuntimeException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}

/// <summary>
/// Represents an exception that indicates some capacity limit was exceeded and thus the request could not be served.
/// This exception is considered retryable after a suitable delay.
/// </summary>
public class ThrottlingException : ServiceException, IRetryableException
{
    public ThrottlingException(string message) : base(message)
    {
        StatusCode = HttpStatusCode.TooManyRequests;
    }
    public ThrottlingException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}

/// <summary>
/// Represents exceptions caused by invalid requests to Fauna.
/// </summary>
public class InvalidRequestException : ServiceException
{
    public InvalidRequestException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}

/// <summary>
/// Represents an exception that occurs when a transaction is aborted due to concurrent modification.
/// This exception is considered retryable after a suitable delay.
/// </summary>
public class ContendedTransactionException : ServiceException, IRetryableException
{
    public ContendedTransactionException(string message, QueryFailure failure) : base(message, failure)
    {
    }
}

// <summary>
// Represents an exception that occurs when a request fails due to a network issue.
// </summary>
public class NetworkException : FaunaException
{
    public string ResponseBody { get; init; }

    public HttpStatusCode StatusCode { get; init; }

    public NetworkException(string message, HttpStatusCode statusCode, string body)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = body;
    }
}