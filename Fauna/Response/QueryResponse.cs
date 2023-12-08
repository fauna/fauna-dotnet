using static Fauna.Constants.ResponseFields;

namespace Fauna;

public abstract class QueryResponse : QueryInfo
{
    internal QueryResponse(string rawResponseText) : base(rawResponseText) { }
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
    public QueryFailure(string rawResponseText) : base(rawResponseText)
    {
    }
}
