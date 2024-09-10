using Fauna.Exceptions;
using Fauna.Mapping;

namespace Fauna.Serialization;

internal class QueryArrSerializer : BaseSerializer<QueryArr>
{
    public override QueryArr Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
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
                writer.WriteStartArray();
                foreach (object? t in o.Unwrap)
                {
                    if (t is IQueryFragment frag)
                    {
                        frag.Serialize(context, writer);
                    }
                    else
                    {
                        var ser = t is not null ? Serializer.Generate(context, t.GetType()) : DynamicSerializer.Singleton;
                        ser.Serialize(context, writer, t);
                    }
                }
                writer.WriteEndArray();
                writer.WriteEndObject();
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(obj.GetType()));
        }
    }
}
