using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;


internal class RefSerializer<T> : BaseSerializer<Ref<T>> where T : notnull
{
    private readonly BaseRefSerializer<T> _baseRefSerializer;

    public RefSerializer(ISerializer docSerializer)
    {
        _baseRefSerializer = new BaseRefSerializer<T>(docSerializer);
    }

    public override Ref<T> Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        return (Ref<T>)_baseRefSerializer.Deserialize(context, ref reader);
    }

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        _baseRefSerializer.Serialize(context, writer, o);
    }
}

internal class NamedRefSerializer<T> : BaseSerializer<NamedRef<T>> where T : notnull
{
    private readonly BaseRefSerializer<T> _baseRefSerializer;

    public NamedRefSerializer(ISerializer docSerializer)
    {
        _baseRefSerializer = new BaseRefSerializer<T>(docSerializer);
    }

    public override NamedRef<T> Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        return (NamedRef<T>)_baseRefSerializer.Deserialize(context, ref reader);
    }

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        _baseRefSerializer.Serialize(context, writer, o);
    }
}

internal class BaseRefSerializer<T> : BaseSerializer<BaseRef<T>> where T : notnull
{
    private readonly ISerializer _docSerializer;

    public BaseRefSerializer(ISerializer docSerializer)
    {
        _docSerializer = docSerializer;
    }

    public override BaseRef<T> Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        return reader.CurrentTokenType switch
        {
            TokenType.StartRef => DeserializeRefInternal(new BaseRefBuilder<T>(), context, ref reader),
            TokenType.StartDocument => DeserializeDocument(new BaseRefBuilder<T>(), context, ref reader),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing into Ref: {reader.CurrentTokenType}")
        };
    }

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case Ref<T> r:
                writer.WriteStartRef();
                writer.WriteString("id", r.Id);
                writer.WriteModule("coll", r.Collection);
                writer.WriteEndRef();
                break;
            case NamedRef<T> r:
                writer.WriteStartRef();
                writer.WriteString("name", r.Name);
                writer.WriteModule("coll", r.Collection);
                writer.WriteEndRef();
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }

    private static BaseRef<T> DeserializeRefInternal(BaseRefBuilder<T> builder, MappingContext context, ref Utf8FaunaReader reader)
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
                case "id":
                    builder.Id = reader.GetString();
                    break;
                case "name":
                    builder.Name = reader.GetString();
                    break;
                case "coll":
                    builder.Collection = reader.GetModule();
                    break;
                case "cause":
                    builder.Cause = reader.GetString();
                    break;
                case "exists":
                    builder.Exists = reader.GetBoolean();
                    break;
                default:
                    throw new SerializationException(
                        $"Unexpected field while deserializing into Ref: {fieldName}");
            }
        }

        return builder.Build();
    }

    public BaseRef<T> DeserializeDocument(BaseRefBuilder<T> builder, MappingContext context, ref Utf8FaunaReader reader)
    {

        while (reader.Read() && reader.CurrentTokenType != TokenType.EndDocument)
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
                case "name":
                    builder.Name = reader.GetString();
                    break;
                case "coll":
                    builder.Collection = reader.GetModule();

                    if (_docSerializer is not IPartialDocumentSerializer cs)
                    {
                        throw new SerializationException($"Serializer {_docSerializer.GetType().Name} must implement IPartialDocumentSerializer interface.");
                    }

                    // This assumes ordering on the wire. If name is not null and we're here, then it's a named document so name is a string.
                    builder.Doc = (T?)cs.DeserializeDocument(context, builder.Id, builder.Name, builder.Collection, ref reader);
                    break;
            }

            // After we deserialize into a doc, we end on the EndDocument a token and do not want to read again
            if (reader.CurrentTokenType == TokenType.EndDocument) break;
        }

        return builder.Build();
    }
}
