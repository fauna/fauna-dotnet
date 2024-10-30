using System.Text.Json;
using Fauna.Core;
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Serialization;
using static Fauna.Core.ResponseFields;

namespace Fauna.Types;


/// <summary>
/// An enum representing Fauna event types.
/// </summary>
public enum EventType
{
    /// <summary>
    /// An add event. E.g. when a document is added to a collection.
    /// </summary>
    Add,
    /// <summary>
    /// An update event. E.g. when a document is updated.
    /// </summary>
    Update,
    /// <summary>
    /// A remove event. E.g. when a document is removed from a collection.
    /// </summary>
    Remove,
    /// <summary>
    /// A status event. Typically used as an implementation detail of a driver. This indicates a status change on the stream.
    /// </summary>
    Status
}

/// <summary>
/// A class representing an event from a stream or feed.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Event<T> where T : notnull
{
    /// <summary>
    /// The type of the event.
    /// </summary>
    public EventType Type { get; private init; }
    /// <summary>
    /// The transaction time of the event.
    /// </summary>
    public long TxnTime { get; private init; }
    /// <summary>
    /// A cursor for restarting at the event.
    /// </summary>
    public string Cursor { get; private init; } = null!;
    /// <summary>
    /// The data associated with the event.
    /// </summary>
    public T? Data { get; private init; }
    /// <summary>
    /// Query stats related to the event.
    /// </summary>
    public QueryStats Stats { get; private init; }

    /// <summary>
    /// A helper method for converting a JSON string into an event.
    /// </summary>
    /// <param name="body">The string of raw JSON.</param>
    /// <param name="ctx">A mapping context to influence deserialization.</param>
    /// <returns>An instance of <see cref="Event{T}"/>.</returns>
    /// <exception cref="FaunaException">Thrown when the event includes a Fauna error.</exception>
    public static Event<T> From(string body, MappingContext ctx)
    {
        var json = JsonSerializer.Deserialize<JsonElement>(body);

        var err = GetError(json);
        if (err != null)
        {
            throw new FaunaException(err.Value);
        }

        var evt = new Event<T>
        {
            TxnTime = GetTxnTime(json),
            Cursor = GetCursor(json),
            Type = GetType(json),
            Stats = GetStats(json),
            Data = GetData(json, ctx),
        };

        return evt;
    }

    private static long GetTxnTime(JsonElement json)
    {
        if (!json.TryGetProperty(LastSeenTxnFieldName, out var elem))
        {
            return default;
        }

        return elem.TryGetInt64(out long i) ? i : default;
    }

    private static string GetCursor(JsonElement json)
    {
        if (!json.TryGetProperty(CursorFieldName, out var elem))
        {
            throw new InvalidDataException($"Missing required field: cursor - {json.ToString()}");
        }

        return elem.Deserialize<string>()!;
    }


    private static EventType GetType(JsonElement json)
    {
        if (!json.TryGetProperty("type", out var elem))
        {
            throw new InvalidDataException($"Missing required field: type - {json.ToString()}");
        }

        string? evtType = elem.Deserialize<string?>();
        EventType type = evtType switch
        {
            "add" => EventType.Add,
            "update" => EventType.Update,
            "remove" => EventType.Remove,
            "status" => EventType.Status,
            _ => throw new InvalidOperationException($"Unknown event type: {evtType}")
        };

        return type;
    }

    private static QueryStats GetStats(JsonElement json)
    {
        return json.TryGetProperty(StatsFieldName, out var elem) ? elem.Deserialize<QueryStats>() : default;
    }

    private static T? GetData(JsonElement json, MappingContext ctx)
    {
        if (!json.TryGetProperty(DataFieldName, out var elem))
        {
            return default;
        }

        var reader = new Utf8FaunaReader(elem.GetRawText());
        reader.Read();

        return Serializer.Generate<T>(ctx).Deserialize(ctx, ref reader);
    }

    private static ErrorInfo? GetError(JsonElement json)
    {
        return json.TryGetProperty(ErrorFieldName, out var elem) ? elem.Deserialize<ErrorInfo>() : null;
    }
}
