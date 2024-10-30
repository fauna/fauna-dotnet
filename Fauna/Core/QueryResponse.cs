using System.Net;
using System.Text.Json;
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Serialization;
using static Fauna.Core.ResponseFields;

namespace Fauna.Core;

/// <summary>
/// Represents the response from a query executed.
/// </summary>
public abstract class QueryResponse
{
    /// <summary>
    /// The raw JSON of the query response.
    /// </summary>
    public JsonElement RawJson { get; init; }

    /// <summary>
    /// Gets the last transaction seen by this query.
    /// </summary>
    public long LastSeenTxn { get; init; }

    /// <summary>
    /// Gets the schema version.
    /// </summary>
    public long SchemaVersion { get; init; }

    /// <summary>
    /// Gets a summary of the query execution.
    /// </summary>
    public string Summary { get; init; } = "";

    /// <summary>
    /// Gets a dictionary of query tags, providing additional context about the query.
    /// </summary>
    public Dictionary<string, string> QueryTags { get; init; } = new();

    /// <summary>
    /// Gets the statistics related to the query execution.
    /// </summary>
    public QueryStats Stats { get; init; }

    internal QueryResponse(JsonElement json)
    {
        RawJson = json;

        if (json.TryGetProperty(LastSeenTxnFieldName, out var elem))
        {
            if (elem.TryGetInt64(out var i)) LastSeenTxn = i;
        }

        if (json.TryGetProperty(SchemaVersionFieldName, out elem))
        {
            if (elem.TryGetInt64(out var i)) LastSeenTxn = i;
        }

        if (json.TryGetProperty(SummaryFieldName, out elem))
        {
            Summary = elem.GetString() ?? "";
        }


        if (json.TryGetProperty(QueryTagsFieldName, out elem))
        {
            var queryTagsString = elem.GetString();

            if (!string.IsNullOrEmpty(queryTagsString))
            {
                var tagPairs = queryTagsString.Split(',').Select(tag =>
                {
                    var tokens = tag.Split('=');
                    return KeyValuePair.Create(tokens[0], tokens[1]);
                });

                QueryTags = new Dictionary<string, string>(tagPairs);
            }
        }

        if (json.TryGetProperty(StatsFieldName, out elem))
        {
            Stats = elem.Deserialize<QueryStats>();
        }
    }

    /// <summary>
    /// Asynchronously parses the HTTP response message to create a QueryResponse instance.
    /// </summary>
    /// <typeparam name="T">The expected data type of the query response.</typeparam>
    /// <param name="ctx">Serialization context for handling response data.</param>
    /// <param name="serializer">A serializer for the success data type.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="body">The response body.</param>
    /// <returns>A Task that resolves to a QueryResponse instance.</returns>
    public static QueryResponse? GetFromResponseBody<T>(
        MappingContext ctx,
        ISerializer<T> serializer,
        HttpStatusCode statusCode,
        string body)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(body);

            if (statusCode is >= HttpStatusCode.OK and <= (HttpStatusCode)299)
            {
                return new QuerySuccess<T>(ctx, serializer, json);
            }

            return new QueryFailure(statusCode, json);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

/// <summary>
/// Represents a successful query response.
/// </summary>
/// <typeparam name="T">The type of data expected in the query result.</typeparam>
public sealed class QuerySuccess<T> : QueryResponse
{
    /// <summary>
    /// Gets the deserialized data from the query response.
    /// </summary>
    public T Data { get; init; }

    /// <summary>
    /// Gets the static type information from the query response, if available.
    /// </summary>
    public string? StaticType { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QuerySuccess{T}"/> class, deserializing the query response into the specified type.
    /// </summary>
    /// <param name="ctx">The serialization context used for deserializing the response data.</param>
    /// <param name="serializer">A deserializer for the response data type.</param>
    /// <param name="json">The parsed JSON response body.</param>
    public QuerySuccess(
        MappingContext ctx,
        ISerializer<T> serializer,
        JsonElement json)
        : base(json)
    {
        var dataText = json.GetProperty(DataFieldName).GetRawText();
        var reader = new Utf8FaunaReader(dataText);
        reader.Read();
        Data = serializer.Deserialize(ctx, ref reader);

        if (json.TryGetProperty(StaticTypeFieldName, out var elem))
        {
            StaticType = elem.GetString();
        }
    }
}

/// <summary>
/// Represents a failed query response.
/// </summary>
public sealed class QueryFailure : QueryResponse
{
    /// <summary>
    /// The HTTP status code.
    /// </summary>
    public HttpStatusCode StatusCode { get; init; }
    /// <summary>
    /// The Fauna error code.
    /// </summary>
    public string ErrorCode { get; init; } = "";
    /// <summary>
    /// The query failure message.
    /// </summary>
    public string Message { get; init; } = "";
    /// <summary>
    /// The constraint failures, if any. Only present for the  constraint_failure error code.
    /// </summary>
    public ConstraintFailure[]? ConstraintFailures { get; init; }
    /// <summary>
    /// The abort object, if any. Only present for the abort error code.
    /// </summary>
    public object? Abort { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryFailure"/> class, parsing the provided raw response text to extract error information.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="json">The JSON response body.</param>
    public QueryFailure(HttpStatusCode statusCode, JsonElement json) : base(json)
    {
        StatusCode = statusCode;
        if (!json.TryGetProperty(ErrorFieldName, out var elem)) return;

        var info = elem.Deserialize<ErrorInfo>();
        ErrorCode = info.Code ?? "";
        Message = info.Message ?? "";

        ConstraintFailures = info.ConstraintFailures;
        Abort = info.Abort;
    }
}
