using Fauna.Mapping;

namespace Fauna.Serialization;

internal class ListSerializer<T> : BaseSerializer<List<T>>
{
    private readonly ISerializer<T> _elemSerializer;

    public ListSerializer(ISerializer<T> elemSerializer)
    {
        _elemSerializer = elemSerializer;
    }

    public override List<T> Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        if (reader.CurrentTokenType == TokenType.StartPage)
            throw new SerializationException(
            $"Unexpected token while deserializing into {typeof(List<T>)}: {reader.CurrentTokenType}");

        var wrapInList = reader.CurrentTokenType != TokenType.StartArray;

        var lst = new List<T>();

        if (wrapInList)
        {
            lst.Add(_elemSerializer.Deserialize(context, ref reader));
        }
        else
        {
            while (reader.Read() && reader.CurrentTokenType != TokenType.EndArray)
            {
                lst.Add(_elemSerializer.Deserialize(context, ref reader));
            }
        }

        return lst;
    }

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        DynamicSerializer.Singleton.Serialize(context, writer, o);
    }
}
