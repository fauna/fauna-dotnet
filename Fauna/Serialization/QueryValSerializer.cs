using System.Diagnostics;
using Fauna.Exceptions;
using Fauna.Mapping;

namespace Fauna.Serialization;


internal class QueryValSerializer : BaseSerializer<QueryObj>
{
    public override QueryObj Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        throw new NotImplementedException();

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? obj)
    {
        switch (obj)
        {
            case null:
                writer.WriteNullValue();
                break;
            case QueryVal v:
                writer.WriteStartObject();
                writer.WriteFieldName("value");
                var ser = v.Unwrap is not null ? Serializer.Generate(context, v.Unwrap.GetType()) : DynamicSerializer.Singleton;
                ser.Serialize(context, writer, v.Unwrap);
                writer.WriteEndObject();
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(obj.GetType()));
        }
    }
}
