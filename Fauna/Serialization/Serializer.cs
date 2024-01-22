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
    /// <param name="context">The context for serialization.</param>
    /// <param name="writer">The writer to serialize the object to.</param>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="typeHint">Optional type hint for the object.</param>
    /// <exception cref="SerializationException">Thrown when serialization fails.</exception>
    public static void Serialize(MappingContext context, Utf8FaunaWriter writer, object? obj, FaunaType? typeHint = null)
    {
        if (typeHint != null)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));

            switch (typeHint)
            {
                case FaunaType.Int:
                    if (obj is byte or sbyte or short or ushort or int)
                    {
                        var int32 = Convert.ToInt32(obj);
                        writer.WriteIntValue(int32);
                    }
                    else
                    {
                        throw new SerializationException($"Unsupported Int conversion. Provided value must be a byte, sbyte, short, ushort, or int but was a {obj.GetType()}");
                    }
                    break;
                case FaunaType.Long:
                    if (obj is byte or sbyte or short or ushort or int or uint or long)
                    {
                        var int64 = Convert.ToInt64(obj);
                        writer.WriteLongValue(int64);
                    }
                    else
                    {
                        throw new SerializationException($"Unsupported Long conversion. Provided value must be a byte, sbyte, short, ushort, int, uint, or long but was a {obj.GetType()}");
                    }
                    break;
                case FaunaType.Double:
                    switch (obj)
                    {
                        case float or double or short or int or long:
                            {
                                var dub = Convert.ToDouble(obj);
                                writer.WriteDoubleValue(dub);
                                break;
                            }
                        default:
                            throw new SerializationException($"Unsupported Double conversion. Provided value must be a short, int, long, float, or double, but was a {obj.GetType()}");
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
                case byte v:
                    writer.WriteIntValue(v);
                    break;
                case sbyte v:
                    writer.WriteIntValue(v);
                    break;
                case ushort v:
                    writer.WriteIntValue(v);
                    break;
                case short v:
                    writer.WriteIntValue(v);
                    break;
                case int v:
                    writer.WriteIntValue(v);
                    break;
                case uint v:
                    writer.WriteLongValue(v);
                    break;
                case long v:
                    writer.WriteLongValue(v);
                    break;
                case float v:
                    writer.WriteDoubleValue(v);
                    break;
                case double v:
                    writer.WriteDoubleValue(v);
                    break;
                case decimal:
                    throw new SerializationException("Decimals are unsupported due to potential loss of precision.");
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

    private static void SerializeObjectInternal(Utf8FaunaWriter writer, object obj, MappingContext context)
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
                    Serialize(context, writer, o);
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
        var mapping = context.Get(t);
        var shouldEscape = mapping.ShouldEscapeObject;

        if (shouldEscape) writer.WriteStartEscapedObject(); else writer.WriteStartObject();
        foreach (var field in mapping.Fields)
        {
            writer.WriteFieldName(field.Name!);
            var v = field.Property.GetValue(obj);
            Serialize(context, writer, v, field.FaunaTypeHint);
        }
        if (shouldEscape) writer.WriteEndEscapedObject(); else writer.WriteEndObject();
    }
}
