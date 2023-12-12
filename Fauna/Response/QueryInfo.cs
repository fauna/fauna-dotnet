using System.Text.Json;
using static Fauna.Constants.ResponseFields;

namespace Fauna;

public class QueryInfo
{
    protected JsonElement _responseBody;

    public long LastSeenTxn { get; init; }
    public int SchemaVersion { get; init; }
    public string Summary { get; init; }
    public Dictionary<string, string>? QueryTags { get; init; }
    public QueryStats Stats { get; init; }

    internal QueryInfo(string rawResponseText)
    {
        _responseBody = JsonSerializer.Deserialize<JsonElement>(rawResponseText);

        LastSeenTxn = _responseBody.GetProperty(LastSeenTxnFieldName).GetInt64();
        SchemaVersion = _responseBody.GetProperty(SchemaVersionFieldName).GetInt32();
        Summary = _responseBody.GetProperty(SummaryFieldName).GetString()!;

        if (_responseBody.TryGetProperty(QueryTagsFieldName, out var queryTagsElement))
        {
            var queryTagsString = queryTagsElement.GetString();

            if (!string.IsNullOrEmpty(queryTagsString))
            {
                var tagPairs = queryTagsString.Split(',').Select(tag => {
                    var tokens = tag.Split('=');
                    return KeyValuePair.Create(tokens[0], tokens[1]);
                });

                QueryTags = new Dictionary<string, string>(tagPairs);
            }
        }

        var statsBlock = _responseBody.GetProperty(StatsFieldName);
        Stats = statsBlock.Deserialize<QueryStats>();
    }
}
