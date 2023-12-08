using System.Collections;

namespace Fauna.Serialization;

public static class Serializer
{
    public static object? Deserialize(string str)
    {
        var reader = new Utf8FaunaReader(str);
        var obj = DeserializeValueInternal(ref reader);
        
        if (reader.Read())
        {
            throw new SerializationException("token stream not exhausted");
        }

        return obj;
    }

    private static object? DeserializeValueInternal(ref Utf8FaunaReader reader)
    {
        object? value;
        
        reader.Read();
        switch (reader.CurrentTokenType)
        {
            case TokenType.StartObject:
                value = DeserializeObjectInternal(ref reader);
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
                value =  null;
                break;
            default:
                throw new SerializationException("unexpected token");
        }

        return value;
    }
    
    private static object? DeserializeObjectInternal(ref Utf8FaunaReader reader)
    {
        reader.Read();
        var obj = new Dictionary<string, object>();
        do
        {
            switch (reader.CurrentTokenType)
            {
                case TokenType.EndObject:
                    return obj;
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