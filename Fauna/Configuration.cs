using Fauna.Core;
using Fauna.Util;
using Microsoft.Extensions.Logging;

namespace Fauna;

/// <summary>
/// Configuration is a class used to configure a Fauna <see cref="Client"/>. It encapsulates various settings such as the <see cref="Endpoint"/>,
/// secret, query timeout, and others.
/// </summary>
public record class Configuration
{
    /// <summary>
    /// Whether the <see cref="Client"/> should dispose of the  <see cref="HttpClient"/> on Dispose.
    /// </summary>
    public bool DisposeHttpClient { get; } = true;

    /// <summary>
    /// Additional buffer to add to <see cref="QueryOptions.QueryTimeout"/> when setting the HTTP request
    /// timeout on the <see cref="HttpClient"/>; default is 5 seconds.
    /// </summary>
    public TimeSpan ClientBufferTimeout { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The HTTP Client to use for requests.
    /// </summary>
    public HttpClient HttpClient { get; init; }

    /// <summary>
    /// The secret key used for authentication.
    /// </summary>
    public string Secret { get; init; } = Environment.GetEnvironmentVariable("FAUNA_SECRET") ?? string.Empty;

    /// <summary>
    /// The endpoint URL of the Fauna server.
    /// </summary>
    public Uri Endpoint { get; init; } = Endpoints.GetFaunaEndpoint();

    /// <summary>
    /// Default options for queries sent to Fauna.
    /// </summary>
    public QueryOptions DefaultQueryOptions { get; init; } = new();

    /// <summary>
    /// The retry configuration to apply to requests.
    /// </summary>
    public RetryConfiguration RetryConfiguration { get; init; } = new(3, TimeSpan.FromSeconds(20));


    /// <summary>
    /// StatsCollector for the client.
    /// </summary>
    public IStatsCollector? StatsCollector { get; init; } = new StatsCollector();

    /// <summary>
    /// Initializes a new instance of the <see cref="Configuration"/> record with the specified secret key.
    /// </summary>
    /// <param name="secret">The secret used for authentication. If null or empty, attempt to use the FAUNA_SECRET env var.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use. If null, a default HttpClient is used.</param>
    /// <param name="logger">A logger. If null, a default logger is used.</param>
    public Configuration(string secret = "", HttpClient? httpClient = null, ILogger? logger = null)
    {
        if (!string.IsNullOrEmpty(secret))
        {
            Secret = secret;
        }

        if (httpClient != null)
        {
            HttpClient = httpClient;
            DisposeHttpClient = false;
        }
        else
        {
            HttpClient = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
        }

        if (logger != null)
        {
            Logger.Initialize(logger);
        }
    }

    internal void Validate()
    {
        if (string.IsNullOrEmpty(Secret))
            throw new ArgumentNullException(
                nameof(Secret),
                "Need to set FAUNA_SECRET environment variable or pass a secret as a parameter when creating the Client."
            );
    }
}
