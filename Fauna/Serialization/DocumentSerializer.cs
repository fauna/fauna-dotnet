using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal class DocumentSerializer : BaseSerializer<Document>
{

    public override Document Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        return reader.CurrentTokenType switch
        {
            TokenType.StartRef or TokenType.StartDocument => Deserialize(new InternalDocument(), context, ref reader, EndTokenFor(reader.CurrentTokenType)),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing into Document: {reader.CurrentTokenType}")
        };
    }

    public static Document Deserialize(InternalDocument builder, MappingContext context, ref Utf8FaunaReader reader, TokenType endToken)
    {
        while (reader.Read() && reader.CurrentTokenType != endToken)
        {
            if (reader.CurrentTokenType != TokenType.FieldName)
                throw new SerializationException(
                    $"Unexpected token while deserializing into NamedRef: {reader.CurrentTokenType}");

            string fieldName = reader.GetString()!;
            reader.Read();
            switch (fieldName)
            {
                case "id":
                    builder.Id = reader.GetString();
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

        return (Document)builder.Get();
    }

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case Document n:
                writer.WriteStartRef();
                writer.WriteString("id", n.Id);
                writer.WriteModule("coll", n.Collection);
                writer.WriteEndRef();
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}
