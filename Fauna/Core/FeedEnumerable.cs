using Fauna.Types;

namespace Fauna.Core;


/// <summary>
/// Represents a Fauna event feed.
/// </summary>
/// <typeparam name="T">Type to map each of the Events to.</typeparam>
public class FeedEnumerable<T> where T : notnull
{
    private readonly BaseClient _client;
    private readonly EventSource _eventSource;
    private readonly CancellationToken _cancel;

    /// <summary>
    /// Current cursor for the Feed.
    /// </summary>
    public string? Cursor => CurrentPage?.Cursor;

    /// <summary>
    /// The latest page returned from the event feed enumerator.
    /// </summary>
    public FeedPage<T>? CurrentPage { get; private set; }

    internal FeedEnumerable(
        BaseClient client,
        EventSource eventSource,
        CancellationToken cancel = default)
    {
        _client = client;
        _eventSource = eventSource;
        _cancel = cancel;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the Feed.
    /// </summary>
    /// <returns>Event Page Enumerator</returns>
    public async IAsyncEnumerator<FeedPage<T>> GetAsyncEnumerator()
    {
        await using var subscribeFeed = _client.SubscribeFeed<T>(
            _eventSource,
            _client.MappingCtx,
            _cancel);

        while (!_cancel.IsCancellationRequested && await subscribeFeed.MoveNextAsync())
        {
            CurrentPage = subscribeFeed.Current;
            yield return CurrentPage;
        }
    }
}
