using System.Collections;
using System.Text.Json;
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Types;
using static Fauna.Core.ResponseFields;

namespace Fauna.Core;

/// <summary>
///
/// </summary>
/// <typeparam name="T"></typeparam>
public class FeedPage<T> where T : notnull
{
    /// <summary>
    ///
    /// </summary>
    public List<Event<T>> Events { get; private init; } = new();

    /// <summary>
    ///
    /// </summary>
    public string Cursor { get; private init; } = null!;

    /// <summary>
    ///
    /// </summary>
    public bool HasNext { get; private init; }

    /// <summary>
    ///
    /// </summary>
    public QueryStats Stats { get; private init; }

    internal static FeedPage<T> From(string body, MappingContext ctx)
    {
        var json = JsonSerializer.Deserialize<JsonElement>(body);

        var err = GetError(json);
        if (err != null)
        {
            throw new FaunaException(err.Value);
        }

        return new FeedPage<T>
        {
            Cursor = GetCursor(json),
            Events = GetEvents(json, ctx),
            Stats = GetStats(json),
            HasNext = json.TryGetProperty(HasNextFieldName, out var elem) && elem.GetBoolean()
        };
    }

    private static List<Event<T>> GetEvents(JsonElement json, MappingContext ctx)
    {
        if (!json.TryGetProperty(EventsFieldName, out var elem))
        {
            return new List<Event<T>>();
        }

        var events = elem.EnumerateArray().Select(e => Event<T>.From(e, ctx)).ToList();
        return events;
    }

    private static QueryStats GetStats(JsonElement json)
    {
        return json.TryGetProperty(StatsFieldName, out var elem) ? elem.Deserialize<QueryStats>() : default;
    }

    private static string GetCursor(JsonElement json)
    {
        return json.TryGetProperty(CursorFieldName, out var elem) ? elem.GetString()! : null!;
    }

    private static ErrorInfo? GetError(JsonElement json)
    {
        return json.TryGetProperty(ErrorFieldName, out var elem) ? elem.Deserialize<ErrorInfo>() : null;
    }
}

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
    /// The last page returned from the Event Feed enumerator.
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

        if (feedOptions?.Cursor is not null)
        {
            _eventSource.LastCursor = feedOptions.Cursor;
        }

        if (feedOptions?.PageSize is > 0)
        {
            _eventSource.PageSize = feedOptions.PageSize;
        }
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
