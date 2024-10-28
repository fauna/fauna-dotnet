using Fauna.Types;

namespace Fauna.Core;

public class StreamEnumerable<T> where T : notnull
{
    private readonly BaseClient _client;
    private readonly EventSource _eventSource;
    private readonly CancellationToken _cancel;

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
