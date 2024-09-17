using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;


internal class QueryExprSerializer : BaseSerializer<QueryExpr>
{
    public override QueryExpr Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        throw new NotImplementedException();

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? obj)
    {
        switch (obj)
        {
            case null:
                writer.WriteNullValue();
                break;
            case QueryExpr o:
                writer.WriteStartObject();
                writer.WriteFieldName("fql");
                writer.WriteStartArray();
                foreach (var t in o.Unwrap)
                {
                    var ser = Serializer.Generate(context, t.GetType());
                    ser.Serialize(context, writer, t);
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(obj.GetType()));
        }
    }
}
