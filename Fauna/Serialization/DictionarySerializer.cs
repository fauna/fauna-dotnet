using System.Text.Json;
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal class DictionarySerializer<T> : BaseSerializer<Dictionary<string, T>>, IPartialDocumentSerializer
{
    private readonly ISerializer<T> _elemSerializer;

    public DictionarySerializer(ISerializer<T> elemSerializer)
    {
        _elemSerializer = elemSerializer;
    }

    public override List<FaunaType> GetSupportedTypes() => new List<FaunaType> { FaunaType.Null, FaunaType.Object };

    public override Dictionary<string, T> Deserialize(MappingContext ctx, ref Utf8FaunaReader reader)
    {
        switch (reader.CurrentTokenType)
        {
            case TokenType.StartObject:
                return DeserializeInternal(new Dictionary<string, T>(), TokenType.EndObject, ctx, ref reader);
            case TokenType.StartDocument:
                return DeserializeInternal(new Dictionary<string, T>(), TokenType.EndDocument, ctx, ref reader);
            default:
                throw new SerializationException(
                    $"Unexpected token while deserializing into {typeof(Dictionary<string, T>)}: {reader.CurrentTokenType}");
        }
    }

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case Dictionary<string, T> d:
                bool shouldEscape = Serializer.Tags.Overlaps(d.Keys);
                if (shouldEscape) writer.WriteStartEscapedObject();
                else writer.WriteStartObject();
                foreach (var (key, value) in d)
                {
                    writer.WriteFieldName(key);
                    _elemSerializer.Serialize(context, writer, value);
                }

                if (shouldEscape) writer.WriteEndEscapedObject();
                else writer.WriteEndObject();
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public object DeserializeDocument(MappingContext context, string? id, string? name, Module? coll, ref Utf8FaunaReader reader)
    {
        var dict = new Dictionary<string, T>();
        if (typeof(T) == typeof(object))
        {
            if (id != null) dict.Add("id", (T)(object)id);
            if (name != null) dict.Add("name", (T)(object)name);
            if (coll != null) dict.Add("coll", (T)(object)coll);
        }

        return DeserializeInternal(dict, TokenType.EndDocument, context, ref reader);
    }

    private Dictionary<string, T> DeserializeInternal(
        Dictionary<string, T> dict,
        TokenType endToken,
        MappingContext context,
        ref Utf8FaunaReader reader)
    {

        while (reader.Read() && reader.CurrentTokenType != endToken)
        {
            if (reader.CurrentTokenType != TokenType.FieldName)
                throw new SerializationException(
                    $"Unexpected token while deserializing field of {typeof(Dictionary<string, T>)}: {reader.CurrentTokenType}");

            string fieldName = reader.GetString()!;
            reader.Read();
            dict.Add(fieldName, _elemSerializer.Deserialize(context, ref reader));
        }

        return dict;
    }
}
