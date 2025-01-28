using Fauna.Types;

namespace Fauna.Core;

/// <summary>
/// Represents the options when subscribing to Fauna event feeds.
/// </summary>
public class FeedOptions : EventOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeedOptions"/> class with the specified cursor and optional page size.
    /// </summary>
    /// <param name="cursor">The cursor for the feed. Used to resume the Feed.</param>
    /// <param name="pageSize">Optional page size for the feed. Sets the maximum number of events returned per page. Must
    /// be in the range 1 to 16000 (inclusive). Defaults to 16.</param>
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
    /// <param name="pageSize">Optional page size for the feed. Sets the maximum number of events returned per page. Must
    /// be in the range 1 to 16000 (inclusive). Defaults to 16.</param>
    /// <seealso href="https://docs.fauna.com/fauna/current/reference/cdc/#get-events-after-a-specific-cursor"/>
    public FeedOptions(long startTs, int? pageSize = null)
    {
        StartTs = startTs;
        PageSize = pageSize;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedOptions"/> class with the specified page size.
    /// </summary>
    /// <param name="pageSize">Maximum number of events returned per page. Must
    /// be in the range 1 to 16000 (inclusive). Defaults to 16.</param>
    public FeedOptions(int pageSize)
    {
        PageSize = pageSize;
    }
}
