using System.Text.Json;
using static Fauna.Constants.ResponseFields;

namespace Fauna;

/// <summary>
/// Provides basic information about a query response, including transaction and schema details.
/// </summary>
public class QueryInfo
{
    protected JsonElement _responseBody;

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
    public string Summary { get; init; }

    /// <summary>
    /// Gets a dictionary of query tags, providing additional context about the query.
    /// </summary>
    public Dictionary<string, string>? QueryTags { get; init; }

    /// <summary>
    /// Gets the statistics related to the query execution.
    /// </summary>
    public QueryStats Stats { get; init; }

    /// <summary>
    /// Initializes a new instance of the QueryInfo class, parsing the provided raw response text to extract common query information.
    /// </summary>
    /// <param name="rawResponseText">The raw JSON response text from the Fauna.</param>
    internal QueryInfo(string rawResponseText)
    {
        _responseBody = JsonSerializer.Deserialize<JsonElement>(rawResponseText);

        LastSeenTxn = _responseBody.GetProperty(LastSeenTxnFieldName).GetInt64();
        SchemaVersion = _responseBody.GetProperty(SchemaVersionFieldName).GetInt64();
        Summary = _responseBody.GetProperty(SummaryFieldName).GetString()!;

        if (_responseBody.TryGetProperty(QueryTagsFieldName, out var queryTagsElement))
        {
            var queryTagsString = queryTagsElement.GetString();

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

        var statsBlock = _responseBody.GetProperty(StatsFieldName);
        Stats = statsBlock.Deserialize<QueryStats>();
    }
}
