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

    /// <summary>
    /// Gets the configured endpoint URI, falling back to the default if not set.
    /// </summary>
    /// <returns>The URI for the Fauna endpoint.</returns>
    public static Uri GetFaunaEndpoint()
    {
        string? endpoint = Environment.GetEnvironmentVariable("FAUNA_ENDPOINT");
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return Default;
        }

        if (Uri.IsWellFormedUriString(endpoint, UriKind.Absolute))
        {
            return new Uri(endpoint);
        }

        throw new UriFormatException("Invalid FAUNA_ENDPOINT environment variable. Must be a valid URI.");
    }
}
