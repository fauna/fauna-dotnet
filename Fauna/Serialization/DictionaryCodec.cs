using Fauna.Mapping;

namespace Fauna.Serialization;

internal class DictionaryCodec<T> : BaseCodec<Dictionary<string, T>>
{
    private readonly ICodec<T> _elemCodec;

    public DictionaryCodec(ICodec<T> elemCodec)
    {
        _elemCodec = elemCodec;
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
            dict.Add(fieldName, _elemCodec.Deserialize(context, ref reader));
        }

        return dict;
    }

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, Dictionary<string, T>? o)
    {
        var shouldEscape = o is not null && Tags.Overlaps(o.Keys);
        if (shouldEscape) writer.WriteStartEscapedObject(); else writer.WriteStartObject();

        if (o is not null)
        {
            foreach (var (key, value) in o)
            {
                writer.WriteFieldName(key);
                _elemCodec.Serialize(context, writer, value);
            }
        }

        if (shouldEscape) writer.WriteEndEscapedObject(); else writer.WriteEndObject();
    }
}
