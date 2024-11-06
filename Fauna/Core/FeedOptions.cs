namespace Fauna.Core;

/// <summary>
/// Represents the options when subscribing to Fauna Event Feeds.
/// </summary>
public class FeedOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeedOptions"/> class with the specified cursor and optional page size.
    /// </summary>
    /// <param name="cursor">The cursor for the feed. Used to resume the Feed.</param>
    /// <param name="pageSize">Optional page size for the feed.</param>
    /// <seealso href="https://docs.fauna.com/fauna/current/reference/cdc/#get-events-after-a-specific-cursor"/>
    public FeedOptions(string cursor, int? pageSize = null)
    {
        Cursor = cursor;
        PageSize = pageSize;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedOptions"/> class with the specified start timestamp and optional page size.
    /// </summary>
    /// <param name="startTs">The start timestamp for the feed. Used to resume the Feed.</param>
    /// <param name="pageSize">Optional page size for the feed.</param>
    /// <seealso href="https://docs.fauna.com/fauna/current/reference/cdc/#get-events-after-a-specific-cursor"/>
    public FeedOptions(long startTs, int? pageSize = null)
    {
        StartTs = startTs;
        PageSize = pageSize;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedOptions"/> class with the specified page size.
    /// </summary>
    /// <param name="pageSize">The page size for the feed.</param>
    public FeedOptions(int pageSize)
    {
        PageSize = pageSize;
    }

    /// <summary>
    /// Cursor returned from Fauna
    /// </summary>
    /// <seealso href="https://docs.fauna.com/fauna/current/reference/cdc/#get-events-after-a-specific-cursor"/>
    public string? Cursor { get; }

    /// <summary>
    /// Start timestamp returned for the feed. Used to resume the Feed.
    /// </summary>
    public long? StartTs { get; }

    /// <summary>
    /// Limit page size for the Feed
    /// </summary>
    public int? PageSize { get; }
}
