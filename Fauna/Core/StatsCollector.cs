namespace Fauna.Core;


public readonly struct Stats
{
    public long ReadOps { get; init; }
    public long ComputeOps { get; init; }
    public long WriteOps { get; init; }
    public long QueryTimeMs { get; init; }
    public int ContentionRetries { get; init; }
    public long StorageBytesRead { get; init; }
    public long StorageBytesWrite { get; init; }
    public int QueryCount { get; init; }

    public int RateLimitedReadQueryCount { get; init; }
    public int RateLimitedComputeQueryCount { get; init; }
    public int RateLimitedWriteQueryCount { get; init; }
}

public interface IStatsCollector
{
    /// <summary>
    /// Add the <see cref="QueryStats"/> to the current counts.
    /// </summary>
    /// <param name="stats">QueryStats</param>
    public void Add(QueryStats stats);

    /// <summary>
    /// Return the collected <see cref="Stats"/>.
    /// </summary>
    public Stats Read();

    /// <summary>
    /// Return the collected <see cref="Stats"/> and Reset counts.
    /// </summary>
    public Stats ReadAndReset();
}

public class StatsCollector : IStatsCollector
{
    private const string RateLimitReadOps = "read";
    private const string RateLimitComputeOps = "compute";
    private const string RateLimitWriteOps = "write";

    private long _readOps;
    private long _computeOps;
    private long _writeOps;
    private long _queryTimeMs;
    private int _contentionRetries;
    private long _storageBytesRead;
    private long _storageBytesWrite;
    private int _queryCount;
    private int _rateLimitedReadQueryCount;
    private int _rateLimitedComputeQueryCount;
    private int _rateLimitedWriteQueryCount;


    public void Add(QueryStats stats)
    {
        Interlocked.Exchange(ref _readOps, _readOps + stats.ReadOps);
        Interlocked.Exchange(ref _computeOps, _computeOps + stats.ComputeOps);
        Interlocked.Exchange(ref _writeOps, _writeOps + stats.WriteOps);
        Interlocked.Exchange(ref _queryTimeMs, _queryTimeMs + stats.QueryTimeMs);
        Interlocked.Exchange(ref _contentionRetries, _contentionRetries + stats.ContentionRetries);
        Interlocked.Exchange(ref _storageBytesRead, _storageBytesRead + stats.StorageBytesRead);
        Interlocked.Exchange(ref _storageBytesWrite, _storageBytesWrite + stats.StorageBytesWrite);

        stats.RateLimitsHit?.ForEach(limitHit =>
        {
            switch (limitHit)
            {
                case RateLimitReadOps:
                    Interlocked.Increment(ref _rateLimitedComputeQueryCount);
                    break;
                case RateLimitComputeOps:
                    Interlocked.Increment(ref _rateLimitedComputeQueryCount);
                    break;
                case RateLimitWriteOps:
                    Interlocked.Increment(ref _rateLimitedWriteQueryCount);
                    break;
            }
        });

        Interlocked.Increment(ref _queryCount);
    }

    public Stats Read()
    {
        return new Stats
        {
            ReadOps = _readOps,
            ComputeOps = _computeOps,
            WriteOps = _writeOps,
            QueryTimeMs = _queryTimeMs,
            ContentionRetries = _contentionRetries,
            StorageBytesRead = _storageBytesRead,
            StorageBytesWrite = _storageBytesWrite,
            QueryCount = _queryCount,
            RateLimitedReadQueryCount = _rateLimitedReadQueryCount,
            RateLimitedComputeQueryCount = _rateLimitedComputeQueryCount,
            RateLimitedWriteQueryCount = _rateLimitedWriteQueryCount
        };
    }

    public Stats ReadAndReset()
    {
        var beforeReset = new Stats
        {
            ReadOps = Interlocked.Exchange(ref _readOps, 0),
            ComputeOps = Interlocked.Exchange(ref _computeOps, 0),
            WriteOps = Interlocked.Exchange(ref _writeOps, 0),
            QueryTimeMs = Interlocked.Exchange(ref _queryTimeMs, 0),
            ContentionRetries = Interlocked.Exchange(ref _contentionRetries, 0),
            StorageBytesRead = Interlocked.Exchange(ref _storageBytesRead, 0),
            StorageBytesWrite = Interlocked.Exchange(ref _storageBytesWrite, 0),
            QueryCount = Interlocked.Exchange(ref _queryCount, 0),
            RateLimitedReadQueryCount = Interlocked.Exchange(ref _rateLimitedReadQueryCount, 0),
            RateLimitedComputeQueryCount = Interlocked.Exchange(ref _rateLimitedComputeQueryCount, 0),
            RateLimitedWriteQueryCount = Interlocked.Exchange(ref _rateLimitedWriteQueryCount, 0)
        };

        return beforeReset;
    }
}
