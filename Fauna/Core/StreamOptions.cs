using Fauna.Types;

namespace Fauna;

/// <summary>
/// Represents the options when subscribing to Fauna Event Streams.
/// </summary>
public class StreamOptions : EventOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamOptions"/> class with the specified token and cursor.
    /// </summary>
    /// <param name="token">The token for a Fauna event source.</param>
    /// <param name="cursor">The cursor from the stream, must be used with the associated Token. Used to resume the stream.</param>
    /// See <a href="https://docs.fauna.com/fauna/current/reference/cdc/#restart">Restart an Event Stream</a>.
    public StreamOptions(string token, string cursor)
    {
        Token = token;
        Cursor = cursor;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamOptions"/> class with the specified token and start timestamp.
    /// </summary>
    /// <param name="token">The token for a Fauna event source.</param>
    /// <param name="startTs">The start timestamp to use for the stream.</param>
    public StreamOptions(string token, long startTs)
    {
        Token = token;
        StartTs = startTs;
    }

    /// <summary>Token for a Fauna event source.</summary>
    /// See the <a
    /// href="https://docs.fauna.com/fauna/current/reference/cdc/#event-source">Create an event source</a>.
    public string? Token { get; }
}
