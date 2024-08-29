using Fauna.Types;
using Stream = Fauna.Types.Stream;

namespace Fauna.Core;

public class StreamEnumerable<T> where T : notnull
{
    private readonly BaseClient _client;
    private readonly Stream _stream;
    private readonly CancellationToken _cancel;

    internal StreamEnumerable(
        BaseClient client,
        Stream stream,
        CancellationToken cancel = default)
    {
        _client = client;
        _stream = stream;
        _cancel = cancel;
    }

    public async IAsyncEnumerator<Event<T>> GetAsyncEnumerator()
    {
        await using var subscribeStream = _client.SubscribeStream<T>(
            _stream,
            _client.MappingCtx,
            _cancel);

        while (!_cancel.IsCancellationRequested && await subscribeStream.MoveNextAsync())
        {
            yield return subscribeStream.Current;
        }
    }
}
