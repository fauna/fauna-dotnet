using Fauna.Exceptions;
using Fauna.Mapping;

namespace Fauna.Serialization.Serializers;

internal class DictionarySerializer<T> : BaseSerializer<Dictionary<string, T>>
{
    private readonly ISerializer<T> _elemSerializer;

    public DictionarySerializer(ISerializer<T> elemSerializer)
    {
        _elemSerializer = elemSerializer;
    }

    public override Dictionary<string, T> Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        if (reader.CurrentTokenType != TokenType.StartObject)
            throw new SerializationException(
                $"Unexpected token while deserializing into {typeof(Dictionary<string, T>)}: {reader.CurrentTokenType}");

        var dict = new Dictionary<string, T>();

        while (reader.Read() && reader.CurrentTokenType != TokenType.EndObject)
        {
            if (reader.CurrentTokenType != TokenType.FieldName)
                throw new SerializationException(
                    $"Unexpected token while deserializing field of {typeof(Dictionary<string, T>)}: {reader.CurrentTokenType}");

            var fieldName = reader.GetString()!;
            reader.Read();
            dict.Add(fieldName, _elemSerializer.Deserialize(context, ref reader));
        }

        return dict;
    }

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        DynamicSerializer.Singleton.Serialize(context, writer, o);
    }
}
