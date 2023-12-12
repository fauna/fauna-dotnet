using System.Reflection;
using System.Text;
using Fauna.Serialization.Attributes;
using Module = Fauna.Types.Module;

namespace Fauna.Serialization;

public static partial class Serializer
{
    
    public static string Serialize(object? obj)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8FaunaWriter(stream);

        var context = new SerializationContext();
        SerializeValueInternal(writer, obj, context);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }
    
    private static void SerializeValueInternal(Utf8FaunaWriter writer, object? obj, SerializationContext context, FaunaType? typeHint = null)
    {
        if (typeHint != null)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));
                
            switch (typeHint)
            {
                case FaunaType.Int:
                    writer.WriteIntValue((int)obj);
                    break;
                case FaunaType.Long:
                    if (obj is int)
                    {
                        var int64 = Convert.ToInt64(obj);
                        writer.WriteLongValue(int64);
                    }
                    else
                    {
                        writer.WriteLongValue((long)obj);
                    }
                    break;
                case FaunaType.Double:
                    writer.WriteDoubleValue((double)obj);
                    break;
                case FaunaType.String:
                    writer.WriteStringValue(obj.ToString() ?? string.Empty);
                    break;
                case FaunaType.Date:
                    if (obj is not DateTime date)
                    {
                        throw new SerializationException($"Unsupported Date conversion. Provided value must be a DateTime but was a {obj.GetType()}");
                    }

                    writer.WriteDateValue(date);
                    break;
                case FaunaType.Time:
                    if (obj is not DateTime time)
                    {
                        throw new SerializationException($"Unsupported Time conversion. Provided value must be a DateTime but was a {obj.GetType()}");
                    }
                    
                    writer.WriteTimeValue(time);
                    break;
                case FaunaType.Boolean:
                    
                    writer.WriteBooleanValue((bool)obj);
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
            case IDictionary<string, string>:
            case IDictionary<string, object>:
                break;
            case IEnumerable<object>:
                break;
            default:
                SerializeClassInternal(writer, obj, context);
                break;
        }
    }

    private static void SerializeClassInternal(Utf8FaunaWriter writer, object obj, SerializationContext context)
    {
        var t = obj.GetType();
        var fieldMap = context.GetFieldMap(t);
        
        writer.WriteStartObject();
        foreach (var field in fieldMap.Values)
        {
            writer.WriteFieldName(field.Name!);
            var v = field.Info?.GetValue(obj);
            SerializeValueInternal(writer, v, context, field.Type);
        }
        writer.WriteEndObject();
    }
}
