using System.Text;
using Module = Fauna.Types.Module;

namespace Fauna.Serialization;

public static partial class Serializer
{

    private static readonly HashSet<string> Tags = new()
    {
        "@int", "@long", "@double", "@date", "@time", "@mod", "@ref", "@doc", "@set", "@object"
    };

    public static string Serialize(object? obj)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8FaunaWriter(stream);
        Serialize(writer, obj);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static void Serialize(Utf8FaunaWriter writer, object? obj)
    {
        var context = new SerializationContext();
        SerializeValueInternal(writer, obj, context);
    }

    private static void SerializeValueInternal(Utf8FaunaWriter writer, object? obj, SerializationContext context, FaunaType? typeHint = null)
    {
        if (typeHint != null)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));

            switch (typeHint)
            {
                case FaunaType.Int:
                    if (obj is short or int)
                    {
                        var int32 = Convert.ToInt32(obj);
                        writer.WriteIntValue(int32);
                    }
                    else
                    {
                        throw new SerializationException($"Unsupported Int conversion. Provided value must be a short or int but was a {obj.GetType()}");
                    }
                    break;
                case FaunaType.Long:
                    if (obj is short or int or long)
                    {
                        var int64 = Convert.ToInt64(obj);
                        writer.WriteLongValue(int64);
                    }
                    else
                    {
                        throw new SerializationException($"Unsupported Long conversion. Provided value must be a short, int, or long but was a {obj.GetType()}");
                    }
                    break;
                case FaunaType.Double:
                    if (obj is double or decimal or short or int or long)
                    {
                        var dec = Convert.ToDecimal(obj);
                        writer.WriteDoubleValue(dec);
                    }
                    else
                    {
                        throw new SerializationException($"Unsupported Double conversion. Provided value must be a short, int, long, double, or decimal, but was a {obj.GetType()}");

                    }
                    break;
                case FaunaType.String:
                    writer.WriteStringValue(obj.ToString() ?? string.Empty);
                    break;
                case FaunaType.Date:
                    switch (obj)
                    {
                        case DateTime v:
                            writer.WriteDateValue(v);
                            break;
                        case DateOnly v:
                            writer.WriteDateValue(v);
                            break;
                        case DateTimeOffset v:
                            writer.WriteDateValue(v);
                            break;
                        default:
                            throw new SerializationException($"Unsupported Date conversion. Provided value must be a DateTime, DateTimeOffset, or DateOnly but was a {obj.GetType()}");
                    }
                    break;
                case FaunaType.Time:
                    switch (obj)
                    {
                        case DateTime v:
                            writer.WriteTimeValue(v);
                            break;
                        case DateTimeOffset v:
                            writer.WriteTimeValue(v);
                            break;
                        default:
                            throw new SerializationException($"Unsupported Time conversion. Provided value must be a DateTime or DateTimeOffset but was a {obj.GetType()}");
                    }
                    break;
                case FaunaType.Boolean:
                    if (obj is bool b)
                    {
                        writer.WriteBooleanValue(b);
                    }
                    else
                    {
                        throw new SerializationException($"Unsupported Boolean conversion. Provided value must be a bool but was a {obj.GetType()}");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeHint), typeHint, null);
            }
        }
        else
        {
            switch (obj)
            {
                case null:
                    writer.WriteNullValue();
                    break;
                case short v:
                    writer.WriteIntValue(v);
                    break;
                case int v:
                    writer.WriteIntValue(v);
                    break;
                case long v:
                    writer.WriteLongValue(v);
                    break;
                case double v:
                    writer.WriteDoubleValue(v);
                    break;
                case decimal v:
                    writer.WriteDoubleValue(v);
                    break;
                case bool v:
                    writer.WriteBooleanValue(v);
                    break;
                case string v:
                    writer.WriteStringValue(v);
                    break;
                case Module v:
                    writer.WriteModuleValue(v);
                    break;
                case DateTime v:
                    writer.WriteTimeValue(v);
                    break;
                case DateTimeOffset v:
                    writer.WriteTimeValue(v);
                    break;
                case DateOnly v:
                    writer.WriteDateValue(v);
                    break;
                default:
                    SerializeObjectInternal(writer, obj, context);
                    break;
            }
        }
    }

    private static void SerializeObjectInternal(Utf8FaunaWriter writer, object obj, SerializationContext context)
    {
        switch (obj)
        {
            case Dictionary<string, object> d:
                SerializeIDictionaryInternal(writer, d, context);
                break;
            case List<object> e:
                writer.WriteStartArray();
                foreach (var o in e)
                {
                    SerializeValueInternal(writer, o, context);
                }
                writer.WriteEndArray();
                break;
            default:
                SerializeClassInternal(writer, obj, context);
                break;
        }
    }

    private static void SerializeIDictionaryInternal<T>(Utf8FaunaWriter writer, IDictionary<string, T> d,
        SerializationContext context)
    {
        var shouldEscape = Tags.Overlaps(d.Keys);
        if (shouldEscape) writer.WriteStartEscapedObject(); else writer.WriteStartObject();
        foreach (var (key, value) in d)
        {
            writer.WriteFieldName(key);
            SerializeValueInternal(writer, value, context);
        }
        if (shouldEscape) writer.WriteEndEscapedObject(); else writer.WriteEndObject();
    }

    private static void SerializeClassInternal(Utf8FaunaWriter writer, object obj, SerializationContext context)
    {
        var t = obj.GetType();
        var fieldMap = context.GetFieldMap(t);
        var shouldEscape = Tags.Overlaps(fieldMap.Values.Select(x => x.Name));

        if (shouldEscape) writer.WriteStartEscapedObject(); else writer.WriteStartObject();
        foreach (var field in fieldMap.Values)
        {
            writer.WriteFieldName(field.Name!);
            var v = field.Info?.GetValue(obj);
            SerializeValueInternal(writer, v, context, field.Type);
        }
        if (shouldEscape) writer.WriteEndEscapedObject(); else writer.WriteEndObject();
    }
}
