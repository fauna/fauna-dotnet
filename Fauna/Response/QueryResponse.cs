using Fauna.Response;
using Fauna.Serialization;
using System;
using System.Text.Json;
using static Fauna.Constants.ResponseFields;

namespace Fauna;

public abstract class QueryResponse : QueryInfo
{
    internal QueryResponse(string rawResponseText) : base(rawResponseText) { }

    public static async Task<QueryResponse> GetFromHttpResponseAsync<T>(HttpResponseMessage message)
    {
        QueryResponse queryResponse;

        var body = await message.Content.ReadAsStringAsync();

        if (!message.IsSuccessStatusCode)
        {
            queryResponse = new QueryFailure(body);
        }
        else
        {
            if (typeof(T) == typeof(Page))
            {
                queryResponse = new QueryPageSuccess(body);
            }
            else
            {
                queryResponse = new QuerySuccess<T>(body);
            }
        }

        return queryResponse;
    }
}

public class QuerySuccess<T> : QueryResponse
{
    public T Data { get; init; }
    public string? StaticType { get; init; }

    public QuerySuccess(string rawResponseText) : base(rawResponseText)
    {
        Data = Serializer.Deserialize<T>(_responseBody.GetProperty(DataFieldName).GetRawText());
        if (_responseBody.TryGetProperty(StaticTypeFieldName, out var jsonElement))
        {
            StaticType = jsonElement.GetString();
        }
    }
}

public class QueryPageSuccess : QueryResponse
{
    public QueryPageInfo Data { get; init; }

    public QueryPageSuccess(string rawResponseText) : base(rawResponseText)
    {
        var dataBlock = _responseBody.GetProperty(DataFieldName);
        Data = dataBlock.Deserialize<QueryPageInfo>();
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
