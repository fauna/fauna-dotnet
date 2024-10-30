namespace Fauna.Core;


/// <summary>
/// A struct representing stats aggregated across queries.
/// </summary>
public readonly struct Stats
{
    /// <summary>
    /// The aggregate read ops.
    /// </summary>
    public long ReadOps { get; init; }
    /// <summary>
    /// The aggregate compute ops.
    /// </summary>
    public long ComputeOps { get; init; }
    /// <summary>
    /// The aggregate write ops.
    /// </summary>
    public long WriteOps { get; init; }
    /// <summary>
    /// The aggregate query time in milliseconds.
    /// </summary>
    public long QueryTimeMs { get; init; }
    /// <summary>
    /// The aggregate number of retries due to transaction contention.
    /// </summary>
    public int ContentionRetries { get; init; }
    /// <summary>
    /// The aggregate number of storage bytes read.
    /// </summary>
    public long StorageBytesRead { get; init; }
    /// <summary>
    /// The aggregate number of storage bytes written.
    /// </summary>
    public long StorageBytesWrite { get; init; }
    /// <summary>
    /// The aggregate number of queries summarized.
    /// </summary>
    public int QueryCount { get; init; }

    /// <summary>
    /// The aggregate count of rate limited queries due to read limits.
    /// </summary>
    public int RateLimitedReadQueryCount { get; init; }
    /// <summary>
    /// The aggregate count of rate limited queries due to compute limits.
    /// </summary>
    public int RateLimitedComputeQueryCount { get; init; }
    /// <summary>
    /// The aggregate count of rate limited queries due to write limits.
    /// </summary>
    public int RateLimitedWriteQueryCount { get; init; }
}

/// <summary>
/// An interface used by a client instance for aggregating stats across all queries.
/// </summary>
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

/// <summary>
/// The default implementation of <see cref="IStatsCollector"/>.
/// </summary>
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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
