namespace Fauna.Constants;

/// <summary>
/// Represents the endpoints used for accessing Fauna.
/// </summary>
public static class Endpoints
{
    /// <summary>
    /// The default URI for Fauna, used for production.
    /// </summary>
    public static Uri Default { get; } = new("https://db.fauna.com");

    /// <summary>
    /// The local development URI for Fauna, used for testing or local development.
    /// </summary>
    public static Uri Local { get; } = new("http://localhost:8443");
}
