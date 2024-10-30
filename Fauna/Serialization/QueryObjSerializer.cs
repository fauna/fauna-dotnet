using Fauna.Exceptions;
using Fauna.Mapping;

namespace Fauna.Serialization;


internal class QueryObjSerializer : BaseSerializer<QueryObj>
{
    public override QueryObj Deserialize(MappingContext ctx, ref Utf8FaunaReader reader) =>
        throw new NotImplementedException();

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? obj)
    {
        switch (obj)
        {
            case null:
                writer.WriteNullValue();
                break;
            case QueryObj o:
                writer.WriteStartObject();
                writer.WriteFieldName("object");
                var ser = Serializer.Generate(context, o.Unwrap.GetType());
                ser.Serialize(context, writer, o.Unwrap);
                writer.WriteEndObject();
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(obj.GetType()));
        }
    }
}
