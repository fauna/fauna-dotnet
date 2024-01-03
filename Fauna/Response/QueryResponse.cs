using Fauna.Serialization;
using System.Text.Json;
using static Fauna.Constants.ResponseFields;

namespace Fauna;

public abstract class QueryResponse : QueryInfo
{
    internal QueryResponse(string rawResponseText) : base(rawResponseText) { }

    public static async Task<QueryResponse> GetFromHttpResponseAsync<T>(SerializationContext ctx, HttpResponseMessage message)
    {
        QueryResponse queryResponse;

        var body = await message.Content.ReadAsStringAsync();

        if (!message.IsSuccessStatusCode)
        {
            queryResponse = new QueryFailure(body);
        }
        else
        {
            queryResponse = new QuerySuccess<T>(ctx, body);
        }

        return queryResponse;
    }
}

public class QuerySuccess<T> : QueryResponse
{
    public T Data { get; init; }
    public string? StaticType { get; init; }

    public QuerySuccess(SerializationContext ctx, string rawResponseText) : base(rawResponseText)
    {
        var dataText = _responseBody.GetProperty(DataFieldName).GetRawText();
        var reader = new Utf8FaunaReader(dataText);
        reader.Read();
        Data = Serializer.Deserialize<T>(ctx, ref reader);

        if (_responseBody.TryGetProperty(StaticTypeFieldName, out var jsonElement))
        {
            StaticType = jsonElement.GetString();
        }
    }
}

public class QueryFailure : QueryResponse
{
    public ErrorInfo ErrorInfo { get; init; }

    public QueryFailure(string rawResponseText) : base(rawResponseText)
    {
        var errorBlock = _responseBody.GetProperty(ErrorFieldName);
        ErrorInfo = errorBlock.Deserialize<ErrorInfo>();
    }
}
