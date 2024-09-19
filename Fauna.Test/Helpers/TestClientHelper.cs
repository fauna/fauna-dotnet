using Fauna.Core;

namespace Fauna.Test.Helpers;

public static class TestClientHelper
{
    public static Client NewTestClient(string secret = "secret", bool hasStatsCollector = true)
    {
        return NewTestClient(secret, "http://localhost:8443", hasStatsCollector);
    }

    public static Client NewTestClient(string secret, string endpoint, bool hasStatsCollector)
    {
        var cfg = new Configuration(secret)
        {
            Endpoint = new Uri(endpoint),
            StatsCollector = hasStatsCollector ? new StatsCollector() : null
        };

        return new Client(cfg);
    }
}
