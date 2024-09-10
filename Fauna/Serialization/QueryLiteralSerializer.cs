using Fauna.Exceptions;
using Fauna.Mapping;

namespace Fauna.Serialization;


internal class QueryLiteralSerializer : BaseSerializer<QueryLiteral>
{
    public override QueryLiteral Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        throw new NotImplementedException();

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? obj)
    {
        switch (obj)
        {
            case null:
                writer.WriteNullValue();
                break;
            case QueryLiteral o:
                writer.WriteStringValue(o.Unwrap);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(obj.GetType()));
        }
    }
}
