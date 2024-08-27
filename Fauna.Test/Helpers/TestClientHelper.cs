using Fauna.Core;

namespace Fauna.Test.Helpers;

public static class TestClientHelper
{
    public static Client NewTestClient(string secret = "secret", bool hasStatsCollector = true)
    {
        var cfg = new Configuration(secret)
        {
            Endpoint = new Uri("http://localhost:8443"),
            StatsCollector = hasStatsCollector ? new StatsCollector() : null
        };

        return new Client(cfg);
    }
}
