namespace Fauna.Types;

/// <summary>
/// Represents a Fauna stream token.
/// </summary>
public class Stream
{
    public Stream(string token)
    {
        Token = token;
    }

    /// <summary>
    /// Gets the string value of the stream token.
    /// </summary>
    public string Token { get; }
}