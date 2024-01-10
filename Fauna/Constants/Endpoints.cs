namespace Fauna.Constants;

/// <summary>
/// Represents the endpoints used for accessing Fauna.
/// </summary>
public readonly struct Endpoints
{
    /// <summary>
    /// The default URI for Fauna, used for production.
    /// </summary>
    public static readonly Uri Default = new("https://db.fauna.com");

    /// <summary>
    /// The local development URI for Fauna, used for testing or local development.
    /// </summary>
    public static readonly Uri Local = new("http://localhost:8443");
}
