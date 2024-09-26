using System.Net;
using Fauna.Core;

namespace Fauna.Exceptions;

/// <summary>
/// Represents an exception related to Fauna service errors, particularly for query failures.
/// </summary>
public class ServiceException : FaunaException
{

    /// <summary>
    /// The error code when a query fails.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// The tags on the x-query-tags header, if it was provided.
    /// </summary>
    public IDictionary<string, string> QueryTags { get; init; }

    /// <summary>
    /// The schema version used by the query. This can be used by clients displaying
    /// schema to determine when they should refresh their schema. If the schema
    /// version that a client has stored differs from the one returned by the query,
    /// schema should be refreshed.
    /// </summary>
    public long? SchemaVersion { get; init; }

    /// <summary>
    /// The query stats for the request.
    /// </summary>
    public QueryStats Stats { get; init; }

    /// <summary>
    /// The HTTP status code.
    /// </summary>
    public HttpStatusCode? StatusCode { get; set; }

    /// <summary>
    /// A comprehensive, human readable summary of any errors, warnings and/or logs returned from the query.
    /// </summary>
    public string? Summary { get; init; }

    /// <summary>
    /// The transaction commit time in micros since epoch. Used by drivers to
    /// populate the x-last-txn-ts request header in order to get a consistent
    /// prefix RYOW guarantee.
    /// </summary>
    public long? TxnTs { get; init; }

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
