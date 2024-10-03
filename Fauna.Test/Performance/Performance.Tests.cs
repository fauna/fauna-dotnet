using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Fauna.Types;
using Fauna.Util.Extensions;
using NUnit.Framework;
using static Fauna.Query;
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

        _client = GetTestClient(secret, endpoint, true);
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
        var metricName = name;
        var query = queryParts.GetCompositedQueryFromParts();

        _stopwatch.Start();

        if (typed && page)
        {
            var singlePage = await _client.QueryAsync<Page<Product>>(query);
            var after = singlePage.Data.After;

            while (after != null)
            {
                foreach (var doc in singlePage.Data.Data)
                {
                    var _ = doc.Name;
                }

                singlePage = await _client.QueryAsync<Page<Product>>(FQL($"Set.paginate({after})"));
                after = singlePage.Data.After;
            }

            var stats = _client.StatsCollector!.ReadAndReset();
            MetricsHandler.RecordMetrics(
                $"{metricName} (query)",
                (int)(_stopwatch.ElapsedMilliseconds / stats.QueryCount),
                (int)(stats.QueryTimeMs / stats.QueryCount)
            );

            _stopwatch.Restart();

            var pages = _client.PaginateAsync<Product>(query);

            await foreach (var p in pages)
            {
                foreach (var doc in p.Data)
                {
                    var _ = doc.Name;
                }
            }

            stats = _client.StatsCollector!.ReadAndReset();
            MetricsHandler.RecordMetrics(
                $"{metricName} (paginate)",
                (int)(_stopwatch.ElapsedMilliseconds / stats.QueryCount),
                (int)(stats.QueryTimeMs / stats.QueryCount)
            );

            _stopwatch.Restart();

            var result = _client.PaginateAsync<Product>(query).FlattenAsync();

            await foreach (var item in result)
            {
                // We can throw this away - just need to iterate
                var _ = item.Name;
            }

            stats = _client.StatsCollector!.ReadAndReset();
            MetricsHandler.RecordMetrics(
                $"{metricName} (flatten)",
                (int)(_stopwatch.ElapsedMilliseconds / stats.QueryCount),
                (int)(stats.QueryTimeMs / stats.QueryCount)
            );
        }
        else if (typed)
        {
            var _ = await _client.QueryAsync<Product>(query);
        }
        else
        {
            var _ = await _client.QueryAsync(query);
        }

        if (!page)
        {
            MetricsHandler.RecordMetrics(
                metricName,
                (int)_stopwatch.ElapsedMilliseconds,
                (int)_client.StatsCollector!.Read().QueryTimeMs
            );
        }
    }
}
