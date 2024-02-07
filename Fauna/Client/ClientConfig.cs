namespace Fauna;

/// <summary>
/// ClientConfig is a configuration class used to set up and configure a connection to Fauna.
/// It encapsulates various settings such as the endpoint URL, secret key, query timeout, and others.
/// </summary>
public record ClientConfig
{
    /// <summary>
    /// The secret key used for authentication.
    /// </summary>
    public string Secret { get; init; }

    /// <summary>
    /// The endpoint URL of the Fauna server.
    /// </summary>
    public Uri Endpoint { get; init; } = Constants.Endpoints.Default;

    /// <summary>
    /// The timeout for the connection.
    /// </summary>
    public TimeSpan ConnectionTimeout { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The maximum number of retry attempts for a request in case of failures.
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// The maximum duration to wait between retry attempts.
    /// </summary>
    public TimeSpan MaxBackoff { get; init; } = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Default options for queries sent to Fauna.
    /// </summary>
    public QueryOptions? DefaultQueryOptions { get; init; } = null;

    /// <summary>
    /// Initializes a new instance of the ClientConfig record with the specified secret key.
    /// </summary>
    /// <param name="secret">The secret key used for authentication.</param>
    public ClientConfig(string secret)
    {
        Secret = secret;
    }
}
