using Fauna.Mapping;

namespace Fauna.Serialization;

internal class ListDeserializer<T> : BaseDeserializer<List<T>>
{
    private IDeserializer<T> _elemDeserializer;

    public ListDeserializer(IDeserializer<T> elemDeserializer)
    {
        _elemDeserializer = elemDeserializer;
    }

    public override List<T> Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        if (reader.CurrentTokenType != TokenType.StartArray)
            throw new SerializationException(
                $"Unexpected token while deserializing into {typeof(List<T>)}: {reader.CurrentTokenType}");

        var lst = new List<T>();
        while (reader.Read() && reader.CurrentTokenType != TokenType.EndArray)
        {
            lst.Add(_elemDeserializer.Deserialize(context, ref reader));
        }

        return lst;
    }
}
