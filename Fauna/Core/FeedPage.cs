using System.Text.Json;
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;
using static Fauna.Core.ResponseFields;

namespace Fauna.Core;

/// <summary>
/// Represents the response from Fauna event feed requests.
/// </summary>
/// <typeparam name="T"></typeparam>
public class FeedPage<T> where T : notnull
{
    /// <summary>
    /// List of Events returned by the Feed
    /// </summary>
    public List<Event<T>> Events { get; private init; } = [];

    /// <summary>
    /// Cursor returned from the Feed
    /// </summary>
    public string Cursor { get; private init; } = null!;

    /// <summary>
    /// Indicates if there are more pages for pagination.
    /// </summary>
    public bool HasNext { get; private init; }

    /// <summary>
    /// Stats returned from the Feed.
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
