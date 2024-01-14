namespace Fauna.Serialization;

internal class DictionaryDeserializer<T> : BaseDeserializer<Dictionary<string, T>>
{
    private IDeserializer<T> _elemDeserializer;

    public DictionaryDeserializer(IDeserializer<T> elemDeserializer)
    {
        _elemDeserializer = elemDeserializer;
    }

    public override Dictionary<string, T> Deserialize(SerializationContext context, ref Utf8FaunaReader reader)
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
            dict.Add(fieldName, _elemDeserializer.Deserialize(context, ref reader));
        }

        return dict;
    }
}
