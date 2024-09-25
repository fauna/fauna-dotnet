using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal class NamedRefSerializer : BaseSerializer<NamedRef>
{

    public override NamedRef Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        return reader.CurrentTokenType switch
        {
            TokenType.StartRef => DeserializeInternal(new InternalDocument(), context, ref reader),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing into NamedRef: {reader.CurrentTokenType}")
        };
    }

    public static NamedRef Deserialize(string? name, MappingContext context, ref Utf8FaunaReader reader)
    {
        InternalDocument builder = new() { Name = name };
        return DeserializeInternal(builder, context, ref reader);
    }

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case NamedRef n:
                writer.WriteStartRef();
                writer.WriteString("name", n.Name);
                writer.WriteModule("coll", n.Collection);
                writer.WriteEndRef();
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }

    private static NamedRef DeserializeInternal(InternalDocument builder, MappingContext context, ref Utf8FaunaReader reader)
    {
        while (reader.Read() && reader.CurrentTokenType != TokenType.EndRef)
        {
            if (reader.CurrentTokenType != TokenType.FieldName)
                throw new SerializationException(
                    $"Unexpected token while deserializing into NamedRef: {reader.CurrentTokenType}");

            string fieldName = reader.GetString()!;
            reader.Read();
            switch (fieldName)
            {
                case "name":
                    builder.Name = reader.GetString();
                    break;
                case "coll":
                    builder.Coll = reader.GetModule();
                    break;
                case "cause":
                    builder.Cause = reader.GetString();
                    break;
                case "exists":
                    builder.Exists = reader.GetBoolean();
                    break;
                default:
                    throw new SerializationException(
                        $"Unexpected field while deserializing into NamedRef: {fieldName}");
            }
        }

        return (NamedRef)builder.Get();
    }

}
