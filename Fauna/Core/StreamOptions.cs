namespace Fauna;

/// <summary>
/// Represents the options when subscribing to Fauna Streams.
/// </summary>
public class StreamOptions
{
    // <summary>Token returned from Fauna when the stream is created.</summary>
    /// <see href="https://docs.fauna.com/fauna/current/reference/http/reference/stream/get/"/>
    public string? Token { get; init; } = null;

    /// <summary>Cursor from the stream, must be used with the associated Token. Used to resume the stream.</summary>
    /// <see href="https://docs.fauna.com/fauna/current/reference/streaming/#restart-a-stream"/>
    public string? Cursor { get; init; } = null;

    // <summary>Start timestamp from the stream, must be used with the associated Token. Used to resume the stream.</summary>
    /// <see href="https://docs.fauna.com/fauna/current/reference/streaming/#restart-a-stream"/>
    public long? StartTs { get; set; } = null;
}
