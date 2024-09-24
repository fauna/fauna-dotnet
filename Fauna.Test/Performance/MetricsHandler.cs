using System.Reflection;
using MathNet.Numerics.Statistics;
using StatsdClient;

namespace Fauna.Test.Performance;

internal class TestTimings
{
    public DateTime CreatedAt { get; init; }
    public int RoundTripMs { get; init; }
    public int QueryTimeMs { get; init; }
    public int OverheadMs { get; init; }

    public TestTimings(int roundTripMs, int queryTimeMs, int overheadMs)
    {
        CreatedAt = DateTime.UtcNow;
        RoundTripMs = roundTripMs;
        QueryTimeMs = queryTimeMs;
        OverheadMs = overheadMs;
    }
}

/// <summary>
/// Provides an abstraction layer to track perf test metrics either to a
/// local CSV file or to Datadog
/// </summary>
internal class MetricsHandler
{
    private static readonly string s_unique = Environment.GetEnvironmentVariable("LOG_UNIQUE") ?? "";
    private static readonly string s_rawStatsFilename = $"rawstats_{s_unique}.csv";
    private static readonly string s_statsBlockFilename = $"stats_{s_unique}.txt";

    public static Dictionary<string, List<TestTimings>> MetricsCollector { get; } = new Dictionary<string, List<TestTimings>>();

    /// <summary>
    /// Whether to send metrics to Datadog instead of a local file
    /// </summary>
    public static bool SendToDatadog
    {
        get { return Environment.GetEnvironmentVariable("SEND_DD_METRICS")! == "true"; }
    }

    /// <summary>
    /// Configures the metrics to either store metrics locally in a CSV file
    /// or send them to Datadog using <see cref="StatsdClient" />; must be called
    /// once before tests are run
    /// </summary>
    public static void ConfigureMetrics()
    {
        if (SendToDatadog)
        {
            // Configure the Datadog Agent metrics endpoint on localhost
            var config = new StatsdConfig
            {
                Prefix = "driver.perf"
            };

            DogStatsd.Configure(config);
        }
    }

    /// <summary>
    /// Record metrics for the given <paramref name="queryName"/> in the configured metrics
    /// collector
    /// </summary>
    /// <param name="queryName">The name of the query to aggregate metrics</param>
    /// <param name="roundTripMs">The full round-trip time in milliseconds of the query and
    /// any pagination/deserialization</param>
    /// <param name="queryTimeMs">The query time in milliseconds as reported by Fauna in the
    /// <see cref="Core.QueryStats"/> from the response</param>
    public static void RecordMetrics(string queryName, int roundTripMs, int queryTimeMs)
    {
        var overhead = roundTripMs - queryTimeMs;

        if (SendToDatadog)
        {
            DogStatsd.Histogram($"{queryName}.latency", roundTripMs, tags: GetMetricsTags());
            DogStatsd.Histogram($"{queryName}.fauna.latency", queryTimeMs, tags: GetMetricsTags());
            DogStatsd.Histogram($"{queryName}.overhead.latency", overhead, tags: GetMetricsTags());
            DogStatsd.Flush();
        }
        else
        {
            if (!MetricsCollector.TryGetValue(queryName, out List<TestTimings>? value))
            {
                value = new List<TestTimings>();
                MetricsCollector.Add(queryName, value);
            }

            value.Add(new TestTimings(roundTripMs, queryTimeMs, overhead));
        }
    }

    /// <summary>
    /// Writes all collected performance metrics to disk
    /// </summary>
    public static void WriteMetricsToFile()
    {
        File.WriteAllLines(s_rawStatsFilename, new[] { "ts,metric,roundTrip,queryTime,diff,tags" });
        File.WriteAllLines(
            s_statsBlockFilename,
            new[] {
                $"{"TEST", -35}{"P50", 9}{"P95", 9}{"P99", 9}{"STDDEV", 9}",
                $"{new string('-', 71)}"
            }
        );

        foreach (var test in MetricsCollector)
        {
            var lines = test.Value.Select(testRun =>
                {
                    var tokens = new[]
                    {
                        testRun.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                        test.Key,
                        testRun.RoundTripMs.ToString(),
                        testRun.QueryTimeMs.ToString(),
                        testRun.OverheadMs.ToString(),
                        string.Join(';', GetMetricsTags())
                    };

                    return string.Join(',', tokens);
                }
            );

            File.AppendAllLines(s_rawStatsFilename, lines);

            double p50 = Math.Round(ArrayStatistics.MedianInplace(test.Value.Select(x => (double)x.OverheadMs).ToArray()), 2);
            double p95 = Math.Round(ArrayStatistics.PercentileInplace(test.Value.Select(x => (double)x.OverheadMs).ToArray(), 95), 2);
            double p99 = Math.Round(ArrayStatistics.PercentileInplace(test.Value.Select(x => (double)x.OverheadMs).ToArray(), 99), 2);
            double stddev = Math.Round(ArrayStatistics.StandardDeviation(test.Value.Select(x => (double)x.OverheadMs).ToArray()), 2);

            File.AppendAllLines(s_statsBlockFilename, new[] { $"{test.Key,-35}{p50,9}{p95,9}{p99,9}{stddev,9}" });
        }
    }

    /// <summary>
    /// Gets a set of tags to be attached to metrics sent to Datadog
    /// </summary>
    /// <returns>An array of strings as "key:value" pairs, e.g. "env:prod"</returns>
    public static string[] GetMetricsTags()
    {
        var env = Environment.GetEnvironmentVariable("FAUNA_ENVIRONMENT") ?? "test";
        var lang = "dotnet";
        var version = Assembly.GetAssembly(typeof(Query))!.GetName().Version ?? new Version(0, 0, 1);

        return new[] { $"env:{env}", $"driver_lang:{lang}", $"version:{version.ToString(3)}" };
    }
}
