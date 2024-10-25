using System.Text.Json;

namespace Fauna.Types;

/// <summary>
/// Represents a Fauna EventSource for initializing Streams and Feeds.
/// </summary>
public sealed class EventSource : IEquatable<EventSource>
{
    public EventSource(string token)
    {
        Token = token;
    }

    /// <summary>
    /// Gets the string value of the stream token.
    /// </summary>
    internal string Token { get; }

    public long? StartTs { get; set; }

    public string? LastCursor { get; set; }

    public void Serialize(System.IO.Stream stream)
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
