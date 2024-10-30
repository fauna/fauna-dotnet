using System.Collections;
using Fauna.Exceptions;
using Fauna.Mapping;
using ArgumentException = System.ArgumentException;

namespace Fauna.Serialization;

internal class ListSerializer<T> : BaseSerializer<List<T>>
{
    private readonly ISerializer<T> _elemSerializer;

    public ListSerializer(ISerializer<T> elemSerializer)
    {
        _elemSerializer = elemSerializer;
    }

    public override List<FaunaType> GetSupportedTypes() => new List<FaunaType> { FaunaType.Array, FaunaType.Null };

    public override List<T> Deserialize(MappingContext ctx, ref Utf8FaunaReader reader)
    {
        if (reader.CurrentTokenType == TokenType.StartPage)
            throw new SerializationException(
            $"Unexpected token while deserializing into {typeof(List<T>)}: {reader.CurrentTokenType}");

        var wrapInList = reader.CurrentTokenType != TokenType.StartArray;

        var lst = new List<T>();

        if (wrapInList)
        {
            lst.Add(_elemSerializer.Deserialize(ctx, ref reader));
        }
        else
        {
            while (reader.Read() && reader.CurrentTokenType != TokenType.EndArray)
            {
                lst.Add(_elemSerializer.Deserialize(ctx, ref reader));
            }
        }

        return lst;
    }

    public override void Serialize(MappingContext ctx, Utf8FaunaWriter w, object? o)
    {
        if (o is null)
        {
            w.WriteNullValue();
            return;
        }

        if (o.GetType().IsGenericType &&
            o.GetType().GetGenericTypeDefinition() == typeof(List<>) ||
            o.GetType().GetGenericTypeDefinition() == typeof(IEnumerable))
        {
            w.WriteStartArray();
            foreach (object? elem in (IEnumerable)o)
            {
                _elemSerializer.Serialize(ctx, w, elem);
            }
            w.WriteEndArray();
            return;
        }

        throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
    }
}
