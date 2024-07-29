namespace Fauna.Types;

/// <summary>
///     Represents a Fauna stream token.
/// </summary>
public sealed class Stream : IEquatable<Stream>
{
    public Stream(string token)
    {
        Token = token;
    }

    /// <summary>
    ///     Gets the string value of the stream token.
    /// </summary>
    public string Token { get; }

    /// <summary>
    ///     Determines whether the specified Stream is equal to the current Stream.
    /// </summary>
    /// <param name="other">The Stream to compare with the current Stream.</param>
    /// <returns>true if the specified Stream is equal to the current Stream; otherwise, false.</returns>
    public bool Equals(Stream? other)
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
        return Equals((Stream)obj);
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
