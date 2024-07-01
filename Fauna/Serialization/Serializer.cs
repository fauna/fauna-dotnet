using Fauna.Mapping;
using Fauna.Mapping.Attributes;
using Module = Fauna.Types.Module;

namespace Fauna.Serialization;

/// <summary>
/// Represents methods for serializing and deserializing objects to and from Fauna format.
/// </summary>
public static partial class Serializer
{

    internal static readonly HashSet<string> Tags = new()
    {
        "@int", "@long", "@double", "@date", "@time", "@mod", "@ref", "@doc", "@set", "@object"
    };

    /// <summary>
    /// Serializes an object to a Fauna compatible format.
    /// </summary>
    /// <param name="ctx">The context for serialization.</param>
    /// <param name="w">The writer to serialize the object to.</param>
    /// <param name="o">The object to serialize.</param>
    /// <exception cref="SerializationException">Thrown when serialization fails.</exception>
    public static void Serialize(MappingContext ctx, Utf8FaunaWriter w, object? o)
    {
        SerializeValueInternal(ctx, w, o);
    }

    private static void SerializeValueInternal(MappingContext ctx, Utf8FaunaWriter w, object? o)
    {
        switch (o)
        {
            case null:
                w.WriteNullValue();
                break;
            case byte v:
                w.WriteIntValue(v);
                break;
            case sbyte v:
                w.WriteIntValue(v);
                break;
            case ushort v:
                w.WriteIntValue(v);
                break;
            case short v:
                w.WriteIntValue(v);
                break;
            case int v:
                w.WriteIntValue(v);
                break;
            case uint v:
                w.WriteLongValue(v);
                break;
            case long v:
                w.WriteLongValue(v);
                break;
            case float v:
                w.WriteDoubleValue(v);
                break;
            case double v:
                w.WriteDoubleValue(v);
                break;
            case decimal:
                throw new SerializationException("Decimals are unsupported due to potential loss of precision.");
            case bool v:
                w.WriteBooleanValue(v);
                break;
            case string v:
                w.WriteStringValue(v);
                break;
            case Module v:
                w.WriteModuleValue(v);
                break;
            case DateTime v:
                w.WriteTimeValue(v);
                break;
            case DateTimeOffset v:
                w.WriteTimeValue(v);
                break;
            case DateOnly v:
                w.WriteDateValue(v);
                break;
            default:
                SerializeObjectInternal(w, o, ctx);
                break;
        }
    }

    private static void SerializeObjectInternal(Utf8FaunaWriter writer, object obj, MappingContext context)
    {
        switch (obj)
        {
            case Dictionary<string, object> d:
                SerializeIDictionaryInternal(writer, d, context);
                break;
            case IEnumerable<object> e:
                writer.WriteStartArray();
                foreach (var o in e)
                {
                    SerializeValueInternal(context, writer, o);
                }
                writer.WriteEndArray();
                break;
            default:
                SerializeClassInternal(writer, obj, context);
                break;
        }
    }

    private static void SerializeIDictionaryInternal<T>(Utf8FaunaWriter writer, IDictionary<string, T> d,
        MappingContext context)
    {
        var shouldEscape = Tags.Overlaps(d.Keys);
        if (shouldEscape) writer.WriteStartEscapedObject(); else writer.WriteStartObject();
        foreach (var (key, value) in d)
        {
            writer.WriteFieldName(key);
            Serialize(context, writer, value);
        }
        if (shouldEscape) writer.WriteEndEscapedObject(); else writer.WriteEndObject();
    }

    private static void SerializeClassInternal(Utf8FaunaWriter writer, object obj, MappingContext context)
    {
        var t = obj.GetType();
        var mapping = context.GetInfo(t);
        var shouldEscape = mapping.ShouldEscapeObject;

        if (shouldEscape) writer.WriteStartEscapedObject(); else writer.WriteStartObject();
        foreach (var field in mapping.Fields)
        {
            writer.WriteFieldName(field.Name!);
            var v = field.Property.GetValue(obj);
            SerializeValueInternal(context, writer, v);
        }
        if (shouldEscape) writer.WriteEndEscapedObject(); else writer.WriteEndObject();
    }
}
