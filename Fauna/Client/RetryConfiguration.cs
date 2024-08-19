using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;
using Polly;
using Polly.Retry;

namespace Fauna;

/// <summary>
/// A class representing a retry configuration for queries.
/// </summary>
public class RetryConfiguration
{

    /// <summary>
    /// Gets the retry policy.
    /// </summary>
    public AsyncPolicy<HttpResponseMessage> RetryPolicy { get; set; }


    /// <summary>
    /// Creates a new <see cref="RetryConfiguration"/> instance.
    /// </summary>
    /// <param name="retryCount">Maximum times to retry a request.</param>
    /// <param name="maxBackoff">The maximum backoff to apply.</param>
    public RetryConfiguration(int retryCount, TimeSpan maxBackoff)
    {
        RetryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<SocketException>()
            .Or<IOException>()
            .Or<InvalidOperationException>()
            .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(retryCount, attempt =>
            {
                int calculated = (int)Math.Floor(Math.Pow(2, attempt));
                int backoff = calculated > maxBackoff.Seconds ? maxBackoff.Seconds : calculated;
                return TimeSpan.FromSeconds(backoff);
            });
    }
}
