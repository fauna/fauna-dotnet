using Fauna.Types;
using Type = System.Type;

namespace Fauna.Serialization;

public static partial class Serializer
{
    public static object? Deserialize(string str)
    {
        return Deserialize(str, null);
    }

    public static T Deserialize<T>(string str)
    {
        return (T)Deserialize(str, typeof(T));
    }

    public static object? Deserialize(string str, Type? type)
    {
        var reader = new Utf8FaunaReader(str);
        var context = new SerializationContext();
        reader.Read();
        var obj = DeserializeValueInternal(ref reader, context, type);

        if (reader.Read())
        {
            throw new SerializationException($"Token stream is not exhausted but should be: {reader.CurrentTokenType}");
        }

        return obj;
    }

    private static T DeserializeValueInternal<T>(ref Utf8FaunaReader reader, SerializationContext context)
    {
        return (T)DeserializeValueInternal(ref reader, context, typeof(T));
    }

    private static object? DeserializeValueInternal(ref Utf8FaunaReader reader, SerializationContext context, Type? targetType = null)
    {
        var value = reader.CurrentTokenType switch
        {
            TokenType.StartObject => DeserializeObjectInternal(ref reader, context, targetType),
            TokenType.StartArray => DeserializeArrayInternal(ref reader, context, targetType),
            TokenType.StartPage => throw new NotImplementedException(),
            TokenType.StartRef => DeserializeRefInternal(ref reader, context, targetType),
            TokenType.StartDocument => DeserializeDocumentInternal(ref reader, context, targetType),
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

    private static object? DeserializeRefInternal(ref Utf8FaunaReader reader, SerializationContext context,
        Type? targetType = null)
    {
        if (targetType != null && targetType != typeof(Ref))
        {
            throw new ArgumentException($"Unsupported target type for ref. Must be a ref or undefined, but was {targetType}");
        }

        var doc = new Ref();
        while (reader.Read() && reader.CurrentTokenType != TokenType.EndRef)
        {
            if (reader.CurrentTokenType == TokenType.FieldName)
            {
                var fieldName = reader.GetString()!;
                reader.Read();
                switch (fieldName)
                {
                    case "id":
                        doc.Id = DeserializeValueInternal<string>(ref reader, context);
                        break;
                    case "coll":
                        doc.Collection = DeserializeValueInternal<Module>(ref reader, context);
                        break;
                }
            }
            else
                throw new SerializationException(
                    $"Unexpected token while deserializing into Document: {reader.CurrentTokenType}");
        }

        return doc;
    }

    private static object? DeserializeDocumentInternal(ref Utf8FaunaReader reader, SerializationContext context,
        Type? targetType = null)
    {
        if (targetType != null && targetType != typeof(Document))
        {
            return DeserializeToClassInternal(ref reader, context, targetType, TokenType.EndDocument);
        }

        var doc = new Document();
        while (reader.Read() && reader.CurrentTokenType != TokenType.EndDocument)
        {
            if (reader.CurrentTokenType == TokenType.FieldName)
            {
                var fieldName = reader.GetString()!;
                reader.Read();
                switch (fieldName)
                {
                    case "id":
                        doc.Id = DeserializeValueInternal<string>(ref reader, context);
                        break;
                    case "ts":
                        doc.Ts = DeserializeValueInternal<DateTime>(ref reader, context);
                        break;
                    case "coll":
                        doc.Collection = DeserializeValueInternal<Module>(ref reader, context);
                        break;
                    default:
                        doc[fieldName] = DeserializeValueInternal(ref reader, context);
                        break;
                }
            }
            else
                throw new SerializationException(
                    $"Unexpected token while deserializing into Document: {reader.CurrentTokenType}");
        }

        return doc;
    }

    private static object? DeserializeArrayInternal(ref Utf8FaunaReader reader, SerializationContext context, Type? targetType = null)
    {
        switch (targetType)
        {
            case null:
            case { IsGenericType: true } when targetType.GetGenericTypeDefinition() == typeof(List<>):
                return DeserializeArrayToListInternal(ref reader, context, targetType);
            default:
                throw new SerializationException(
                    $"Unsupported target type for array. Must be an List<> or unspecified, but was {targetType}");
        }
    }

    private static object? DeserializeArrayToListInternal(ref Utf8FaunaReader reader, SerializationContext context,
        Type? targetType)
    {
        if (targetType == null)
        {
            var lst = new List<object?>();
            while (reader.Read() && reader.CurrentTokenType != TokenType.EndArray)
            {
                lst.Add(DeserializeValueInternal(ref reader, context));
            }
            return lst;
        }
        else
        {
            var lst = Activator.CreateInstance(targetType);
            var elementType = targetType.GetGenericArguments().Single();
            var add = targetType.GetMethod("Add")!;

            while (reader.Read() && reader.CurrentTokenType != TokenType.EndArray)
            {
                var parameters = new[]
                {
                    DeserializeValueInternal(ref reader, context, elementType)
                };
                add.Invoke(lst, parameters);
            }

            return lst;
        }
    }

    private static object? DeserializeObjectInternal(ref Utf8FaunaReader reader, SerializationContext context, Type? targetType = null)
    {
        switch (targetType)
        {
            case null:
            case { IsGenericType: true } when targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>):
                return DeserializeObjectToDictionaryInternal(ref reader, context, targetType);
            default:
                return DeserializeToClassInternal(ref reader, context, targetType, TokenType.EndObject);
        }
    }

    private static object? DeserializeToClassInternal(ref Utf8FaunaReader reader, SerializationContext context, Type t, TokenType endToken)
    {
        var fieldMap = context.GetFieldMap(t);
        var instance = Activator.CreateInstance(t);

        while (reader.Read() && reader.CurrentTokenType != endToken)
        {
            if (reader.CurrentTokenType == TokenType.FieldName)
            {
                var fieldName = reader.GetString()!;
                reader.Read();

                if (fieldMap.ContainsKey(fieldName))
                {
                    fieldMap[fieldName].Info!.SetValue(instance, DeserializeValueInternal(ref reader, context)!);
                }
                else
                {
                    reader.Read();
                    reader.Skip();
                }
            }
            else
            {
                throw new SerializationException($"Unexpected token while deserializing into class {t.Name}: {reader.CurrentTokenType}");
            }
        }

        return instance;
    }

    private static object? DeserializeObjectToDictionaryInternal(ref Utf8FaunaReader reader, SerializationContext context, Type? targetType = null)
    {

        if (targetType == null)
        {
            var obj = new Dictionary<string, object>();

            while (reader.Read() && reader.CurrentTokenType != TokenType.EndObject)
            {
                if (reader.CurrentTokenType == TokenType.FieldName)
                {
                    var fieldName = reader.GetString()!;
                    reader.Read();
                    obj[fieldName] = DeserializeValueInternal(ref reader, context)!;
                }
                else
                    throw new SerializationException(
                        $"Unexpected token while deserializing into dictionary: {reader.CurrentTokenType}");
            }

            return obj;
        }
        else
        {
            var obj = Activator.CreateInstance(targetType);
            var argTypes = targetType.GetGenericArguments();
            if (argTypes.Length != 2)
            {
                throw new ArgumentException($"Unsupported generic type: {targetType}");
            }

            var keyType = argTypes[0];
            if (keyType != typeof(string))
            {
                throw new ArgumentException(
                    $"Unsupported Dictionary key type. Key must be of type string, but was a {keyType}");
            }

            var valueType = argTypes[1];
            var add = targetType.GetMethod("Add")!;

            while (reader.Read() && reader.CurrentTokenType != TokenType.EndObject)
            {

                if (reader.CurrentTokenType == TokenType.FieldName)
                {
                    var fieldName = reader.GetString()!;
                    reader.Read();
                    var parameters = new[]
                    {
                        fieldName,
                        DeserializeValueInternal(ref reader, context, valueType)
                    };
                    add.Invoke(obj, parameters);
                }
                else
                    throw new SerializationException(
                        $"Unexpected token while deserializing into dictionary: {reader.CurrentTokenType}");
            }

            return obj;
        }
    }
}
