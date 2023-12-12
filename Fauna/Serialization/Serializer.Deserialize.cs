using System.Collections;
using System.Reflection;
using Fauna.Serialization.Attributes;
using Type = System.Type;

namespace Fauna.Serialization;

public static partial class Serializer
{
    public static object? Deserialize(string str)
    {
        return Deserialize(str, typeof(object));
    }
    
    public static T? Deserialize<T>(string str)
    {
        return (T?)Deserialize(str, typeof(T));
    }

    public static object? Deserialize(string str, Type type)
    {
        var reader = new Utf8FaunaReader(str);
        var context = new SerializationContext();
        var obj = DeserializeValueInternal(ref reader, context, type);

        if (reader.Read())
        {
            throw new SerializationException($"Token stream is not exhausted but should be: {reader.CurrentTokenType}");
        }

        return obj;
    }

    private static object? DeserializeValueInternal(ref Utf8FaunaReader reader, SerializationContext context, Type? targetType = null)
    {
        reader.Read();
        
        var value = reader.CurrentTokenType switch
        {
            TokenType.StartObject => DeserializeObjectInternal(ref reader, context, targetType),
            TokenType.StartArray => throw new NotImplementedException(),
            TokenType.StartSet => throw new NotImplementedException(),
            TokenType.StartRef => throw new NotImplementedException(),
            TokenType.StartDocument => throw new NotImplementedException(),
            TokenType.String => reader.GetValue(),
            TokenType.Int => reader.GetValue(),
            TokenType.Long => reader.GetValue(),
            TokenType.Double => reader.GetValue(),
            TokenType.Date => reader.GetValue(),
            TokenType.Time => reader.GetValue(),
            TokenType.True => reader.GetValue(),
            TokenType.False => reader.GetValue(),
            TokenType.Module => reader.GetValue(),
            TokenType.Null => null,
            _ => throw new SerializationException(
                $"Unexpected token while deserializing: {reader.CurrentTokenType}")
        };

        return value;
    }

    private static object? DeserializeObjectInternal(ref Utf8FaunaReader reader, SerializationContext context, Type? targetType = null)
    {
        return targetType == null || targetType == typeof(object) ? DeserializeDictionaryInternal(ref reader, context) : DeserializeClassInternal(ref reader, context, targetType);
    }
    
    private static object? DeserializeClassInternal(ref Utf8FaunaReader reader, SerializationContext context, Type t)
    {
        var fieldMap = context.GetFieldMap(t);
        var instance = Activator.CreateInstance(t);

        while (reader.Read() && reader.CurrentTokenType != TokenType.EndObject)
        {
            if (reader.CurrentTokenType == TokenType.FieldName)
            {
                var fieldName = reader.GetString()!;
                if (fieldMap.ContainsKey(fieldName))
                {
                    fieldMap[fieldName].Info!.SetValue(instance, DeserializeValueInternal(ref reader, context)!);
                }
                else
                {
                    // Test DeserializeIntoPocoWithAttributes fails until we have Skip()
                    // reader.Read();
                    // reader.Skip();
                }
            }
            else
            {
                throw new SerializationException($"Unexpected token while deserializing into class {t.Name}: {reader.CurrentTokenType}");
            }
        }

        return instance;
    }

    private static object? DeserializeDictionaryInternal(ref Utf8FaunaReader reader, SerializationContext context)
    {
        var obj = new Dictionary<string, object>();

        while (reader.Read() && reader.CurrentTokenType != TokenType.EndObject)
        {
            if (reader.CurrentTokenType == TokenType.FieldName)
                obj[reader.GetString()!] = DeserializeValueInternal(ref reader, context)!;
            else
                throw new SerializationException(
                    $"Unexpected token while deserializing into dictionary: {reader.CurrentTokenType}");
        }

        return obj;
    }

}