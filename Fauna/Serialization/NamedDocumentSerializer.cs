using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal class NamedDocumentSerializer : BaseSerializer<NamedDocument>
{

    public override NamedDocument Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        return reader.CurrentTokenType switch
        {
            TokenType.StartRef or TokenType.StartDocument => Deserialize(new InternalDocument(), context, ref reader, EndTokenFor(reader.CurrentTokenType)),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing into NamedDocument: {reader.CurrentTokenType}")
        };
    }

    public static NamedDocument Deserialize(InternalDocument builder, MappingContext context, ref Utf8FaunaReader reader, TokenType endToken)
    {
        while (reader.Read() && reader.CurrentTokenType != endToken)
        {
            if (reader.CurrentTokenType != TokenType.FieldName)
                throw new SerializationException(
                    $"Unexpected token while deserializing into NamedDocument: {reader.CurrentTokenType}");

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
                case "ts":
                    builder.Ts = reader.GetTime();
                    break;
                case "cause":
                    builder.Cause = reader.GetString();
                    break;
                case "exists":
                    builder.Exists = reader.GetBoolean();
                    break;
                default:
                    builder.Data[fieldName] = DynamicSerializer.Singleton.Deserialize(context, ref reader);
                    break;
            }
        }

        return (NamedDocument)builder.Get();
    }

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case NamedDocument n:
                writer.WriteStartRef();
                writer.WriteString("name", n.Name);
                writer.WriteModule("coll", n.Collection);
                writer.WriteEndRef();
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}
