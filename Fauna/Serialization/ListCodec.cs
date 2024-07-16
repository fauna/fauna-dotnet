using Fauna.Mapping;

namespace Fauna.Serialization;

internal class ListCodec<T> : BaseCodec<List<T>>
{
    private readonly ICodec<T> _elemCodec;

    public ListCodec(ICodec<T> elemCodec)
    {
        _elemCodec = elemCodec;
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
            lst.Add(_elemCodec.Deserialize(context, ref reader));
        }
        else
        {
            while (reader.Read() && reader.CurrentTokenType != TokenType.EndArray)
            {
                lst.Add(_elemCodec.Deserialize(context, ref reader));
            }
        }

        return lst;
    }

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, List<T>? o)
    {
        writer.WriteStartArray();
        if (o is not null)
        {
            foreach (var elem in o)
            {
                _elemCodec.Serialize(context, writer, elem);
            }
        }
        writer.WriteEndArray();
    }
}
