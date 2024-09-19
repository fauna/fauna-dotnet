using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Fauna.Util.Extensions;
using NUnit.Framework;
using static Fauna.Test.Helpers.TestClientHelper;

namespace Fauna.Test.Performance;

[TestFixture]
[Category("Performance")]
[Explicit]
public class PerformanceTests
{
    [AllowNull]
    private static Client _client = null!;

    private readonly Stopwatch _stopwatch = new();

    [OneTimeSetUp]
    public void BeforeAll()
    {
        MetricsHandler.ConfigureMetrics();

        var secret = Environment.GetEnvironmentVariable("FAUNA_SECRET") ?? "secret";
        var endpoint = Environment.GetEnvironmentVariable("FAUNA_ENDPOINT") ?? "http://localhost:8443";

        _client = NewTestClient(secret, endpoint, true);
    }

    [SetUp]
    public void Before()
    {
        var _ = _client.StatsCollector!.ReadAndReset();
    }

    [TearDown]
    public void After()
    {
        _stopwatch.Reset();
    }

    [OneTimeTearDown]
    public void AfterAll()
    {
        _client.Dispose();
        MetricsHandler.WriteMetricsToFile();
    }

    [Test]
    [Repeat(20)]
    [TestCaseSource(typeof(TestDataParser), nameof(TestDataParser.GetQueriesFromFile))]
    public async Task ExecuteQueryAndCollectStats(string name, List<string> queryParts, bool typed, bool page)
    {
        var query = queryParts.GetCompositedQueryFromParts();

        _stopwatch.Start();

        if (typed && page)
        {
            var result = _client.PaginateAsync<Product>(query).FlattenAsync();

            await foreach (var item in result)
            {
                // We can throw this away - just need to iterate
                var _ = item.Name;
            }
        }
        else if (typed)
        {
            var _ = await _client.QueryAsync<Product>(query);
        }
        else
        {
            var _ = await _client.QueryAsync(query);
        }

        _stopwatch.Stop();

        MetricsHandler.RecordMetric(name, (int)_stopwatch.ElapsedMilliseconds, (int)_client.StatsCollector!.Read().QueryTimeMs);
    }
}
