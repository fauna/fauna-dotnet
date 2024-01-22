using Fauna.Mapping;
using Fauna.Serialization;
using System.Text.Json;
using static Fauna.Constants.ResponseFields;

namespace Fauna;

/// <summary>
/// Represents the response from a query executed.
/// </summary>
public abstract class QueryResponse : QueryInfo
{
    internal QueryResponse(string rawResponseText) : base(rawResponseText) { }

    /// <summary>
    /// Asynchronously parses the HTTP response message to create a QueryResponse instance.
    /// </summary>
    /// <typeparam name="T">The expected data type of the query response.</typeparam>
    /// <param name="ctx">Serialization context for handling response data.</param>
    /// <param name="deserializer">A deserializer for the success data type.</param>
    /// <param name="message">The HTTP response message received from the Fauna database.</param>
    /// <returns>A Task that resolves to a QueryResponse instance.</returns>
    public static async Task<QueryResponse> GetFromHttpResponseAsync<T>(
        MappingContext ctx,
        IDeserializer<T> deserializer,
        HttpResponseMessage message)
    {
        QueryResponse queryResponse;

        var body = await message.Content.ReadAsStringAsync();

        if (!message.IsSuccessStatusCode)
        {
            queryResponse = new QueryFailure(body);
        }
        else
        {
            queryResponse = new QuerySuccess<T>(ctx, deserializer, body);
        }

        return queryResponse;
    }
}

/// <summary>
/// Represents a successful query response.
/// </summary>
/// <typeparam name="T">The type of data expected in the query result.</typeparam>
public class QuerySuccess<T> : QueryResponse
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
    /// <param name="deserializer">A deserializer for the response data type.</param>
    /// <param name="rawResponseText">The raw JSON response text from the Fauna database.</param>
    public QuerySuccess(
        MappingContext ctx,
        IDeserializer<T> deserializer,
        string rawResponseText)
        : base(rawResponseText)
    {
        var dataText = _responseBody.GetProperty(DataFieldName).GetRawText();
        var reader = new Utf8FaunaReader(dataText);
        reader.Read();
        Data = deserializer.Deserialize(ctx, ref reader);

        if (_responseBody.TryGetProperty(StaticTypeFieldName, out var jsonElement))
        {
            StaticType = jsonElement.GetString();
        }
    }
}

/// <summary>
/// Represents a failed query response.
/// </summary>
public class QueryFailure : QueryResponse
{
    /// <summary>
    /// Gets the error information associated with the failed query.
    /// </summary>
    public ErrorInfo ErrorInfo { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryFailure"/> class, parsing the provided raw response text to extract error information.
    /// </summary>
    /// <param name="rawResponseText">The raw JSON response text from the Fauna that contains the error information.</param>
    public QueryFailure(string rawResponseText) : base(rawResponseText)
    {
        var errorBlock = _responseBody.GetProperty(ErrorFieldName);
        ErrorInfo = errorBlock.Deserialize<ErrorInfo>();
    }
}
