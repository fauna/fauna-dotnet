using MathNet.Numerics.Statistics;
using StatsdClient;

namespace Fauna.Test.Performance;

internal class TestTimings
{
    public DateTime CreatedAt { get; init; }
    public int RoundTripMs { get; init; }
    public int QueryTimeMs { get; init; }
    public int OverheadMs { get; init; }

    public TestTimings(int roundTripMs, int queryTimeMs)
    {
        CreatedAt = DateTime.UtcNow;
        RoundTripMs = roundTripMs;
        QueryTimeMs = queryTimeMs;
        OverheadMs = roundTripMs - queryTimeMs;
    }
}

/// <summary>
/// Provides an abstraction layer to track perf test metrics either to a
/// local CSV file or to Datadog
/// </summary>
internal class MetricsHandler
{
    private const string RawStatsFilename = "rawstats.csv";
    private const string StatsBlockFilename = "stats.txt";

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
                StatsdServerName = "127.0.0.1",
                StatsdPort = 8125,
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
    public static void RecordMetric(string queryName, int roundTripMs, int queryTimeMs)
    {
        var overhead = roundTripMs - queryTimeMs;

        if (SendToDatadog)
        {
            DogStatsd.Gauge($"{queryName}.round_trip", roundTripMs, tags: GetMetricsTags());
            DogStatsd.Gauge($"{queryName}.query_time", queryTimeMs, tags: GetMetricsTags());
            DogStatsd.Gauge($"{queryName}.overhead", overhead, tags: GetMetricsTags());
            DogStatsd.Flush();
        }
        else
        {
            if (!MetricsCollector.TryGetValue(queryName, out List<TestTimings>? value))
            {
                value = new List<TestTimings>();
                MetricsCollector.Add(queryName, value);
            }

            value.Add(new TestTimings(roundTripMs, queryTimeMs));
        }
    }

    /// <summary>
    /// Writes all collected performance metrics to disk
    /// </summary>
    public static void WriteMetricsToFile()
    {
        File.WriteAllLines(RawStatsFilename, new[] { "ts,metric,roundTrip,queryTime,diff" });
        File.WriteAllLines(
            StatsBlockFilename,
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
                        testRun.OverheadMs.ToString()
                    };

                    return string.Join(',', tokens);
                }
            );

            File.AppendAllLines(RawStatsFilename, lines);

            double p50 = Math.Round(ArrayStatistics.MedianInplace(test.Value.Select(x => (double)x.OverheadMs).ToArray()), 2);
            double p95 = Math.Round(ArrayStatistics.PercentileInplace(test.Value.Select(x => (double)x.OverheadMs).ToArray(), 95), 2);
            double p99 = Math.Round(ArrayStatistics.PercentileInplace(test.Value.Select(x => (double)x.OverheadMs).ToArray(), 99), 2);
            double stddev = Math.Round(ArrayStatistics.StandardDeviation(test.Value.Select(x => (double)x.OverheadMs).ToArray()), 2);

            File.AppendAllLines(StatsBlockFilename, new[] { $"{test.Key,-35}{p50,9}{p95,9}{p99,9}{stddev,9}" });
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

        return new[] { $"env:{env}", $"driver_lang:{lang}" };
    }
}
