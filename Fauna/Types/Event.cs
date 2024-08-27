using System.Text.Json;
using Fauna.Core;
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Serialization;
using static Fauna.Core.ResponseFields;

namespace Fauna.Types;


public enum EventType
{
    Add,
    Update,
    Remove,
    Status
}

public class Event<T> where T : notnull
{
    public EventType Type { get; private init; }
    public long TxnTime { get; private init; }
    public T? Data { get; private init; }
    public QueryStats Stats { get; private init; }

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

    private static EventType GetType(JsonElement json)
    {
        if (!json.TryGetProperty("type", out var elem))
        {
            throw new Exception($"Missing required field: type - {json.ToString()}");
        }

        string? evtType = elem.Deserialize<string?>();
        EventType type = evtType switch
        {
            "add" => EventType.Add,
            "update" => EventType.Update,
            "remove" => EventType.Remove,
            "status" => EventType.Status,
            _ => throw new Exception($"Unknown event type: {evtType}")
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
