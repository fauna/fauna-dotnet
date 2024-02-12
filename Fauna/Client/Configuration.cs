namespace Fauna;

/// <summary>
/// Configuration is a class used to configure a Fauna client. It encapsulates various settings such as the endpoint,
/// secret key, query timeout, and others.
/// </summary>
public record Configuration
{
    /// <summary>
    /// The HTTP Client to use for requests.
    /// </summary>
    public HttpClient HttpClient { get; init; }

    /// <summary>
    /// The secret key used for authentication.
    /// </summary>
    public string Secret { get; init; }

    /// <summary>
    /// The endpoint URL of the Fauna server.
    /// </summary>
    public Uri Endpoint { get; set; } = Constants.Endpoints.Default;

    /// <summary>
    /// Default options for queries sent to Fauna.
    /// </summary>
    public QueryOptions? DefaultQueryOptions { get; init; } = null;

    /// <summary>
    /// The retry configuration to apply to requests.
    /// </summary>
    public RetryConfiguration RetryConfiguration { get; init; } = new(3, TimeSpan.FromSeconds(20));

    /// <summary>
    /// Initializes a new instance of the ClientConfig record with the specified secret key.
    /// </summary>
    /// <param name="secret">The secret key used for authentication.</param>
    /// <param name="httpClient">The HTTP Client to use.</param>
    public Configuration(string secret, HttpClient? httpClient = null)
    {
        HttpClient = httpClient ?? new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
        Secret = secret;
    }
}
