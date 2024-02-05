using Fauna.Mapping;

namespace Fauna.Serialization;

internal interface ListDeserializer
{
    public IDeserializer Elem { get; }
}

internal class ListDeserializer<T> : BaseDeserializer<List<T>>, ListDeserializer
{
    public IDeserializer<T> Elem { get; }
    IDeserializer ListDeserializer.Elem { get => Elem; }

    public ListDeserializer(IDeserializer<T> elem)
    {
        Elem = elem;
    }

    public override List<T> Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        if (reader.CurrentTokenType != TokenType.StartArray)
            throw new SerializationException(
                $"Unexpected token while deserializing into {typeof(List<T>)}: {reader.CurrentTokenType}");

        var lst = new List<T>();
        while (reader.Read() && reader.CurrentTokenType != TokenType.EndArray)
        {
            lst.Add(Elem.Deserialize(context, ref reader));
        }

        return lst;
    }
}
