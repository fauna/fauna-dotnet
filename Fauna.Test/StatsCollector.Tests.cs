using Fauna.Core;
using NUnit.Framework;

namespace Fauna.Test;

public class StatsCollectorTests
{
    [Test]
    public void StatsCollectorConcurrentReadsAndWrites()
    {
        var statsCollector = new StatsCollector();
        var random = new Random();

        const int cycles = 100;

        Parallel.For(0, cycles, i =>
        {
            var writeStats = new QueryStats
            {
                ReadOps = random.Next(1, int.MaxValue),
                ComputeOps = random.Next(1, int.MaxValue),
                WriteOps = random.Next(1, int.MaxValue),
                QueryTimeMs = random.Next(1, int.MaxValue),
                ContentionRetries = random.Next(1, int.MaxValue),
                StorageBytesRead = random.Next(1, int.MaxValue),
                StorageBytesWrite = random.Next(1, int.MaxValue),
                RateLimitsHit = new List<string>()
            };
            statsCollector.Add(writeStats);
        });

        Parallel.For(0, cycles, i =>
        {
            var currentValue = statsCollector.Read();
            Assert.NotZero(currentValue.ReadOps);
            Assert.NotZero(currentValue.ComputeOps);
            Assert.NotZero(currentValue.WriteOps);
            Assert.NotZero(currentValue.QueryTimeMs);
            Assert.NotZero(currentValue.ContentionRetries);
            Assert.NotZero(currentValue.StorageBytesRead);
            Assert.NotZero(currentValue.StorageBytesWrite);
            Assert.NotZero(currentValue.QueryCount);
        });
    }
}
