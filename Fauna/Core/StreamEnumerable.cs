using Fauna.Types;

namespace Fauna.Core;

/// <summary>
/// A class representing a Fauna Event Stream. Additional queries will be made during enumeration.
/// </summary>
/// <typeparam name="T">The return type of the stream.</typeparam>
public class StreamEnumerable<T> where T : notnull
{
    private readonly BaseClient _client;
    private readonly EventSource _eventSource;
    private readonly CancellationToken _cancel;

    /// <summary>
    /// The token for the event source.
    /// </summary>
    public string Token => _eventSource.Token;

    internal StreamEnumerable(
        BaseClient client,
        EventSource eventSource,
        CancellationToken cancel = default)
    {
        _client = client;
        _eventSource = eventSource;
        _cancel = cancel;
    }

    /// <summary>
    /// Gets an async enumerator for the stream.
    /// </summary>
    /// <returns>An async enumerator that yields <see cref="Event{T}"/>.</returns>
    public async IAsyncEnumerator<Event<T>> GetAsyncEnumerator()
    {
        await using var subscribeStream = _client.SubscribeStream<T>(
            _eventSource,
            _client.MappingCtx,
            _cancel);

        while (!_cancel.IsCancellationRequested && await subscribeStream.MoveNextAsync())
        {
            yield return subscribeStream.Current;
        }
    }
}
