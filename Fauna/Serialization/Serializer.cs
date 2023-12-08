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
            throw new SerializationException("token stream not exhausted");
        }

        return obj;
    }

    private static object? DeserializeValueInternal(ref Utf8FaunaReader reader, Type? targetType = null)
    {
        object? value;

        reader.Read();
        switch (reader.CurrentTokenType)
        {
            case TokenType.StartObject:
                value = DeserializeObjectInternal(ref reader, targetType);
                break;
            case TokenType.StartArray:
                throw new NotImplementedException();
            case TokenType.StartSet:
                throw new NotImplementedException();
            case TokenType.StartRef:
                throw new NotImplementedException();
            case TokenType.StartDocument:
                throw new NotImplementedException();
            case TokenType.String:
            case TokenType.Int:
            case TokenType.Long:
            case TokenType.Double:
            case TokenType.Date:
            case TokenType.Time:
            case TokenType.True:
            case TokenType.False:
            case TokenType.Module:
                value = reader.GetValue();
                break;
            case TokenType.Null:
                value = null;
                break;
            default:
                throw new SerializationException("unexpected token");
        }

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
                propMap[attr.GetName()] = prop;
            }
            else
            {
                propMap[prop.Name] = prop;
            }
        }

        do
        {
            if (reader.CurrentTokenType == TokenType.EndObject) break;

            switch (reader.CurrentTokenType)
            {
                case TokenType.FieldName:
                    var fieldName = reader.GetString()!;
                    if (propMap.ContainsKey(fieldName))
                    {
                        propMap[fieldName].SetValue(instance, DeserializeValueInternal(ref reader)!);
                    }
                    break;
                default:
                    throw new SerializationException("unexpected token");
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
                    throw new SerializationException("unexpected token");
            }
        } while (reader.Read());

        return obj;
    }

}