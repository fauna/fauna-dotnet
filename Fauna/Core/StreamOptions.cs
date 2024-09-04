namespace Fauna;

/// <summary>
/// Represents the options when subscribing to Fauna Streams.
/// </summary>
public class StreamOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StreamOptions"/> class with the specified token and cursor.
    /// </summary>
    /// <param name="token">The token returned from Fauna when the stream is created.</param>
    /// <param name="cursor">The cursor from the stream, must be used with the associated Token. Used to resume the stream.</param>
    /// <seealso href="https://docs.fauna.com/fauna/current/reference/streaming/#restart-a-stream"/>
    public StreamOptions(string token, string cursor)
    {
        Token = token;
        Cursor = cursor;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamOptions"/> class with the specified token and start timestamp.
    /// </summary>
    /// <param name="token">The token returned from Fauna when the stream is created.</param>
    /// <param name="startTs">The start timestamp to use for the stream.</param>
    public StreamOptions(string token, long startTs)
    {
        Token = token;
        StartTs = startTs;
    }

    // <summary>Token returned from Fauna when the stream is created.</summary>
    /// <see href="https://docs.fauna.com/fauna/current/reference/http/reference/stream/get/"/>
    public string? Token { get; }

    /// <summary>Cursor from the stream, must be used with the associated Token. Used to resume the stream.</summary>
    /// <see href="https://docs.fauna.com/fauna/current/reference/streaming/#restart-a-stream"/>
    public string? Cursor { get; }

    // <summary>Start timestamp from the stream, must be used with the associated Token. Used to resume the stream.</summary>
    /// <see href="https://docs.fauna.com/fauna/current/reference/streaming/#restart-a-stream"/>
    public long? StartTs { get; }
}
