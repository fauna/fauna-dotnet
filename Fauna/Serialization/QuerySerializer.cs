using Fauna.Exceptions;
using Fauna.Mapping;

namespace Fauna.Serialization;

internal class QuerySerializer : BaseSerializer<Query>
{
    public override Query Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        throw new NotImplementedException();

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? obj)
    {
        switch (obj)
        {
            case null:
                writer.WriteNullValue();
                break;
            case Query o:
                var ser = Serializer.Generate(context, o.GetType());
                ser.Serialize(context, writer, o);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(obj.GetType()));
        }
    }
}
