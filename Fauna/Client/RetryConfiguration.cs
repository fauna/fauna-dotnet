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


    public IAsyncPolicy StreamRetryPolicy { get; set; }


    /// <summary>
    /// Creates a new <see cref="RetryConfiguration"/> instance.
    /// </summary>
    /// <param name="retryCount">Maximum times to retry a request.</param>
    /// <param name="maxBackoff">The maximum backoff to apply.</param>
    public RetryConfiguration(int retryCount, TimeSpan maxBackoff)
    {
        RetryPolicy = DefaultRetryPolicy(retryCount, maxBackoff);
        StreamRetryPolicy = DefaultStreamRetryPolicy(retryCount, maxBackoff);
    }

    private AsyncPolicy<HttpResponseMessage> DefaultRetryPolicy(int retryCount, TimeSpan maxBackoff)
    {
        return Policy
            .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(retryCount, attempt =>
            {
                int calculated = (int)Math.Floor(Math.Pow(2, attempt));
                int backoff = calculated > maxBackoff.Seconds ? maxBackoff.Seconds : calculated;
                return TimeSpan.FromSeconds(backoff);
            });
    }

    private IAsyncPolicy DefaultStreamRetryPolicy(int retryCount, TimeSpan maxBackoff)
    {
        ImmutableArray<Type> networkExceptions = new[]
        {
            typeof(SocketException),
            typeof(IOException)
        }.ToImmutableArray();

        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions()
            {
                MaxRetryAttempts = retryCount,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = args =>
                {
                    if (args.Outcome.Exception != null)
                    {
                        Exception ex = args.Outcome.Exception;
                        bool retry = networkExceptions.Contains(ex.GetBaseException().GetType());

                        return new ValueTask<bool>(retry);
                    }

                    if (args.Outcome.Result == null)
                    {
                        return new ValueTask<bool>(false);
                    }

                    switch (args.Outcome.Result)
                    {
                        case HttpResponseMessage response:
                            if (response.StatusCode is HttpStatusCode.TooManyRequests
                                or HttpStatusCode.ServiceUnavailable)
                                return new ValueTask<bool>(true);
                            break;
                    }

                    return new ValueTask<bool>(false);
                },
                DelayGenerator = arguments =>
                {
                    int calculated = (int)Math.Floor(Math.Pow(2, arguments.AttemptNumber));
                    int backoff = calculated > maxBackoff.Seconds ? maxBackoff.Seconds : calculated;
                    return new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(backoff));
                },
            })
            .Build()
            .AsAsyncPolicy();
    }
}
