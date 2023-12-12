using System.Text.Json;
using static Fauna.Constants.ResponseFields;

namespace Fauna;

public abstract class QueryResponse : QueryInfo
{
    internal QueryResponse(string rawResponseText) : base(rawResponseText) { }

    public static async Task<QueryResponse> GetFromHttpResponseAsync<T>(HttpResponseMessage message)
        where T : class
    {
        QueryResponse queryResponse;

        var body = await message.Content.ReadAsStringAsync();

        if (!message.IsSuccessStatusCode)
        {
            queryResponse = new QueryFailure(body);
        }
        else
        {
            queryResponse = new QuerySuccess<T>(body);
        }

        return queryResponse;
    }
}

public class QuerySuccess<T> : QueryResponse where T : class
{
    public T Data { get; init; }
    public string? StaticType { get; init; }

    public QuerySuccess(string rawResponseText) : base(rawResponseText)
    {
        Data = _responseBody.GetProperty(DataFieldName).GetRawText() as T;
        
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
