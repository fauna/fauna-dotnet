namespace Fauna.Core;

/// <summary>
/// Represents the endpoints used for accessing Fauna.
/// </summary>
public static class Endpoints
{
    /// <summary>
    /// The default URI for Fauna, used for production.
    /// </summary>
    public static Uri Default { get; } = new("https://db.fauna.com");
}
