using System.Text.Json;

namespace Fauna.Types;

/// <summary>
/// Represents a Fauna EventSource for initializing Streams and Feeds.
/// </summary>
public sealed class EventSource : IEquatable<EventSource>
{
    /// <summary>
    /// Gets the string value of the stream token.
    /// </summary>
    internal string Token { get; }

    internal EventOptions Options { get; set; }

    /// <summary>
    /// Initializes an <see cref="EventSource"/>.
    /// </summary>
    /// <param name="token">An event source.</param>
    public EventSource(string token)
    {
        Token = token;
        Options = new EventOptions();
    }

    /// <summary>
    /// Serializes the event source to the provided <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream"></param>
    public void Serialize(Stream stream)
    {
        var writer = new Utf8JsonWriter(stream);
        writer.WriteStartObject();
        writer.WriteString("token", Token);
        if (Options.Cursor != null)
        {
            writer.WriteString("cursor", Options.Cursor);
        }
        else if (Options.StartTs != null)
        {
            writer.WriteNumber("start_ts", Options.StartTs.Value);
        }

        if (Options.PageSize is > 0)
        {
            writer.WriteNumber("page_size", Options.PageSize.Value);
        }

        writer.WriteEndObject();
        writer.Flush();
    }


    /// <summary>
    ///     Determines whether the specified Stream is equal to the current Stream.
    /// </summary>
    /// <param name="other">The Stream to compare with the current Stream.</param>
    /// <returns>true if the specified Stream is equal to the current Stream; otherwise, false.</returns>
    public bool Equals(EventSource? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Token == other.Token;
    }

    /// <summary>
    ///     Determines whether the specified object is equal to the current Stream.
    /// </summary>
    /// <param name="obj">The object to compare with the current Stream.</param>
    /// <returns>true if the specified object is equal to the current Stream; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((EventSource)obj);
    }

    /// <summary>
    ///     The default hash function.
    /// </summary>
    /// <returns>A hash code for the current Stream.</returns>
    public override int GetHashCode()
    {
        return Token.GetHashCode();
    }
}

/// <summary>
/// Represents the options for a Fauna EventSource.
/// </summary>
public class EventOptions
{
    /// <summary>
    /// Cursor returned from Fauna
    /// </summary>
    /// <seealso href="https://docs.fauna.com/fauna/current/reference/cdc/#get-events-after-a-specific-cursor"/>
    public string? Cursor { get; internal set; }

    /// <summary>
    /// Start timestamp returned for the feed. Used to resume the Feed.
    /// </summary>
    public long? StartTs { get; protected init; }

    /// <summary>
    /// Limit page size for the Feed
    /// </summary>
    public int? PageSize { get; protected init; }
}
