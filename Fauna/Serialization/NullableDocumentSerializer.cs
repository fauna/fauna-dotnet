using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal class NullableDocumentSerializer<T> : BaseSerializer<NullableDocument<T>> where T : class
{
    private readonly ISerializer<T> _valueSerializer;

    public NullableDocumentSerializer(ISerializer<T> valueSerializer)
    {
        _valueSerializer = valueSerializer;
    }

    public override NullableDocument<T> Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        if (reader.CurrentTokenType is not (TokenType.StartObject or TokenType.StartRef or TokenType.StartDocument))
            throw new SerializationException(
                $"Unexpected token while deserializing into {typeof(NullableDocument<T>)}: {reader.CurrentTokenType}");

        try
        {
            var val = _valueSerializer.Deserialize(context, ref reader);
            return new NonNullDocument<T>(val);
        }
        catch (NullDocumentException e)
        {
            return new NullDocument<T>(e.Id, e.Name, e.Collection, e.Cause);
        }
    }

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case NonNullDocument<T> n:
                _valueSerializer.Serialize(context, writer, n.Value);
                break;
            case NullDocument<T> n:
                if (n.Id != null)
                {
                    writer.WriteStartRef();
                    writer.WriteString("id", n.Id);
                    writer.WriteModule("coll", n.Collection);
                    writer.WriteEndRef();
                }
                else
                {
                    writer.WriteStartRef();
                    writer.WriteString("name", n.Name!);
                    writer.WriteModule("coll", n.Collection);
                    writer.WriteEndRef();
                }
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}
