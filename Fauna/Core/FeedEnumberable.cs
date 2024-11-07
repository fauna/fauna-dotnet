using Fauna.Types;

namespace Fauna.Core;


/// <summary>
/// Represents a Fauna Event Feed.
/// </summary>
/// <typeparam name="T">Type to map each of the Events to.</typeparam>
public class FeedEnumerable<T> where T : notnull
{
    private readonly BaseClient _client;
    private readonly EventSource _eventSource;
    private readonly CancellationToken _cancel;
    private readonly FeedOptions? _feedOptions;

    /// <summary>
    /// The current cursor for the Feed.
    /// </summary>
    public string? Cursor => _eventSource.LastCursor;

    /// <summary>
    /// The latest page returned from the Event Feed enumerator.
    /// </summary>
    public FeedPage<T>? CurrentPage { get; private set; }

    internal FeedEnumerable(
        BaseClient client,
        EventSource eventSource,
        FeedOptions? feedOptions = null,
        CancellationToken cancel = default)
    {
        _client = client;
        _eventSource = eventSource;
        _cancel = cancel;
        _feedOptions = feedOptions;

        _eventSource.LastCursor = feedOptions?.Cursor;
        _eventSource.StartTs = feedOptions?.StartTs;
        _eventSource.PageSize = feedOptions?.PageSize;
    }

    /// <summary>
    /// Move to the next page of the Event Feed.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> NextAsync()
    {
        await using var subscribeFeed = _client.SubscribeFeed<T>(
            _eventSource,
            _client.MappingCtx,
            _cancel);

        bool result = await subscribeFeed.MoveNextAsync();
        if (result)
        {
            CurrentPage = subscribeFeed.Current;
        }

        return result;
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
