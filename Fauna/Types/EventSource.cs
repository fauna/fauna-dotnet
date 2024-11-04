using System.Text.Json;

namespace Fauna.Types;

/// <summary>
/// Represents a Fauna EventSource for initializing Streams and Feeds.
/// </summary>
public sealed class EventSource : IEquatable<EventSource>
{
    /// <summary>
    /// Initializes an <see cref="EventSource"/>.
    /// </summary>
    /// <param name="token">An event source.</param>
    public EventSource(string token)
    {
        Token = token;
    }

    /// <summary>
    /// Gets the string value of the stream token.
    /// </summary>
    internal string Token { get; }

    /// <summary>
    /// The start timestamp of the Event Feed or Event Stream.
    /// </summary>
    public long? StartTs { get; set; }

    /// <summary>
    /// The starting cursor for the Event Feed or Event Stream. Typically, this is the last observed cursor.
    /// </summary>
    public string? LastCursor { get; set; }

    /// <summary>
    /// Set the page size when using Event Feeds.
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Serializes the event source to the provided <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream"></param>
    public void Serialize(Stream stream)
    {
        var writer = new Utf8JsonWriter(stream);
        writer.WriteStartObject();
        writer.WriteString("token", Token);
        if (LastCursor != null)
        {
            writer.WriteString("cursor", LastCursor);
        }
        else if (StartTs != null)
        {
            writer.WriteNumber("start_ts", StartTs.Value);
        }

        if (PageSize is > 0)
        {
            writer.WriteNumber("page_size", PageSize.Value);
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
