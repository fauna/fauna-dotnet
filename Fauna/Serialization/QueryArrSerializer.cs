using Fauna.Exceptions;
using Fauna.Mapping;

namespace Fauna.Serialization;

internal class QueryArrSerializer : BaseSerializer<QueryArr>
{
    public override QueryArr Deserialize(MappingContext ctx, ref Utf8FaunaReader reader) =>
        throw new NotImplementedException();

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? obj)
    {
        switch (obj)
        {
            case null:
                writer.WriteNullValue();
                break;
            case QueryArr o:
                writer.WriteStartObject();
                writer.WriteFieldName("array");
                var ser = Serializer.Generate(context, o.Unwrap.GetType());
                ser.Serialize(context, writer, o.Unwrap);
                writer.WriteEndObject();
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(obj.GetType()));
        }
    }
}
