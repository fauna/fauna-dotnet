using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;

namespace Fauna.Test.Performance;

/// <summary>
/// Representation of each query object as defined in './setup/queries.json'
/// </summary>
internal class TestQuery
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = "";
    [JsonPropertyName("parts")]
    public List<string> Parts { get; init; } = new List<string>();
    [JsonPropertyName("response")]
    public TestResponse? Response { get; init; }
}

/// <summary>
/// Representation of each query's response object as defined in './setup/queries.json'
/// </summary>
internal class TestResponse
{
    [JsonPropertyName("typed")]
    public bool Typed { get; init; } = false;
    [JsonPropertyName("page")]
    public bool Page { get; init; } = false;
}

/// <summary>
/// Representation of the root "queries" object as defined in './setup/queries.json'
/// </summary>
internal class TestQueries
{
    [JsonPropertyName("queries")]
    public List<TestQuery> Queries { get; init; } = new List<TestQuery>();
}

/// <summary>
/// A static class that provides for parsing queries from 'queries.json' and passing
/// the arguments to <see cref="PerformanceTests"/>
/// </summary>
internal static class TestDataParser
{
    /// <summary>
    /// Parse the 'queries.json' file and pass deserialized parameters as data source args
    /// to <see cref="PerformanceTests"/>
    /// </summary>
    /// <returns>An IEnumerable<TestCaseData> that iterates over test scenarios</returns>
    public static IEnumerable<TestCaseData> GetQueriesFromFile()
    {
        var contents = File.ReadAllText("./utils/queries.json");
        var jsonRoot = JsonSerializer.Deserialize<TestQueries>(contents);

        foreach (var query in jsonRoot!.Queries)
        {
            yield return new TestCaseData(
                query.Name,
                query.Parts,
                query.Response?.Typed,
                query.Response?.Page);
        }
    }
}
