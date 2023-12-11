using System.Collections;
using System.Reflection;
using Fauna.Serialization.Attributes;
using Type = System.Type;

namespace Fauna.Serialization;

public static class Serializer
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
        var obj = DeserializeValueInternal(ref reader, type);

        if (reader.Read())
        {
            throw new SerializationException($"Token stream is not exhausted but should be: {reader.CurrentTokenType}");
        }

        return obj;
    }

    private static object? DeserializeValueInternal(ref Utf8FaunaReader reader, Type? targetType = null)
    {
        reader.Read();
        
        var value = reader.CurrentTokenType switch
        {
            TokenType.StartObject => DeserializeObjectInternal(ref reader, targetType),
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

    private static object? DeserializeObjectInternal(ref Utf8FaunaReader reader, Type? targetType = null)
    {
        reader.Read();
        return targetType == null || targetType == typeof(object) ? DeserializeDictionaryInternal(ref reader) : DeserializeClassInternal(ref reader, targetType);
    }
    
    private static object? DeserializeClassInternal(ref Utf8FaunaReader reader, Type t)
    {
        var instance = Activator.CreateInstance(t);
        var propMap = new Dictionary<string, PropertyInfo>();
        var props = t.GetProperties();

        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<FaunaFieldName>();
            if (attr != null)
            {
                propMap[attr.Name] = prop;
            }
            else
            {
                propMap[prop.Name] = prop;
            }
        }

        do
        {
            if (reader.CurrentTokenType == TokenType.EndObject)
            {
                break;
            }

            if (reader.CurrentTokenType == TokenType.FieldName)
            {
                var fieldName = reader.GetString()!;
                if (propMap.ContainsKey(fieldName))
                {
                    propMap[fieldName].SetValue(instance, DeserializeValueInternal(ref reader)!);
                }
            }
            else
            {
                throw new SerializationException($"Unexpected token while deserializing into class {t.Name}: {reader.CurrentTokenType}");
            }
        } while (reader.Read());

        return instance;
    }

    private static object? DeserializeDictionaryInternal(ref Utf8FaunaReader reader)
    {
        var obj = new Dictionary<string, object>();
        do
        {
            if (reader.CurrentTokenType == TokenType.EndObject) break;
            
            switch (reader.CurrentTokenType)
            {
                case TokenType.FieldName:
                    obj[reader.GetString()!] = DeserializeValueInternal(ref reader)!;
                    break;
                default:
                    throw new SerializationException($"Unexpected token while deserializing into dictionary: {reader.CurrentTokenType}");
            }
        } while (reader.Read());

        return obj;
    }

}