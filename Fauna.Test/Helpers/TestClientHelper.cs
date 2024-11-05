using Fauna.Core;

namespace Fauna.Test.Helpers;

public static class TestClientHelper
{
    public static Client GetLocalhostClient(string secret = "secret", bool hasStatsCollector = true, bool debugMode = false)
    {
        if (debugMode)
            Environment.SetEnvironmentVariable("FAUNA_DEBUG", "0");

        return GetTestClient(secret, "http://localhost:8443", hasStatsCollector);
    }

    public static Client GetTestClient(string secret, string endpoint, bool hasStatsCollector)
    {
        var cfg = new Configuration(secret)
        {
            Endpoint = new Uri(endpoint),
            StatsCollector = hasStatsCollector ? new StatsCollector() : null
        };

        return new Client(cfg);
    }
}
