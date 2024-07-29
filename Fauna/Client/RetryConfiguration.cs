using System.Net;
using Polly;

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
            .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(retryCount, attempt =>
            {
                var calculated = (int)Math.Floor(Math.Pow(2, attempt));
                var backoff = calculated > maxBackoff.Seconds ? maxBackoff.Seconds : calculated;
                return TimeSpan.FromSeconds(backoff);
            });
    }

}
