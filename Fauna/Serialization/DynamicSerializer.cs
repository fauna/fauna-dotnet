using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal class DynamicSerializer : BaseSerializer<object?>
{
    public static DynamicSerializer Singleton { get; } = new();

    private readonly ListSerializer<object?> _list;
    private readonly PageSerializer<object?> _page;
    private readonly DictionarySerializer<object?> _dict;
    private readonly QuerySerializer _query;


    private DynamicSerializer()
    {
        _list = new ListSerializer<object?>(this);
        _page = new PageSerializer<object?>(this);
        _dict = new DictionarySerializer<object?>(this);
        _query = new QuerySerializer();
    }

    public override object? Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.StartObject => _dict.Deserialize(context, ref reader),
            TokenType.StartArray => _list.Deserialize(context, ref reader),
            TokenType.StartPage => _page.Deserialize(context, ref reader),
            TokenType.StartRef => DeserializeRefInternal(context, ref reader),
            TokenType.StartDocument => DeserializeDocumentInternal(context, ref reader),
            TokenType.String => reader.GetString(),
            TokenType.Int => reader.GetInt(),
            TokenType.Long => reader.GetLong(),
            TokenType.Double => reader.GetDouble(),
            TokenType.Date => reader.GetDate(),
            TokenType.Time => reader.GetTime(),
            TokenType.True or TokenType.False => reader.GetBoolean(),
            TokenType.Module => reader.GetModule(),
            TokenType.Null => null,
            _ => throw new SerializationException(
                $"Unexpected token while deserializing: {reader.CurrentTokenType}"),
        };

    private static object DeserializeRefInternal(MappingContext context, ref Utf8FaunaReader reader)
    {
        reader.Read();

        if (reader.CurrentTokenType != TokenType.FieldName)
            throw new SerializationException(
                $"Unexpected token while deserializing @ref: {reader.CurrentTokenType}");

        string fieldName = reader.GetString()!;
        reader.Read();

        switch (fieldName)
        {
            case "id":
                try
                {
                    return RefSerializer.Deserialize(reader.GetString(), context, ref reader);
                }
                catch (NullDocumentException e)
                {
                    return new NullDocument<Document>(e.Id, null, e.Collection, e.Cause);
                }

            case "name":
                try
                {
                    return NamedRefSerializer.Deserialize(reader.GetString(), context, ref reader);
                }
                catch (NullDocumentException e)
                {
                    return new NullDocument<NamedDocument>(null, e.Name, e.Collection, e.Cause);
                }

            default:
                throw new SerializationException($"Unexpected field while deserializing @ref: {fieldName}");
        }
    }

    private static object DeserializeDocumentInternal(MappingContext context, ref Utf8FaunaReader reader)
    {
        var builder = new InternalDocument();
        while (reader.Read() && reader.CurrentTokenType != TokenType.EndDocument)
        {
            if (reader.CurrentTokenType != TokenType.FieldName)
                throw new SerializationException(
                    $"Unexpected token while deserializing @doc: {reader.CurrentTokenType}");

            string fieldName = reader.GetString()!;

            reader.Read();

            try
            {
                switch (fieldName)
                {
                    // Relies on ordering for doc fields.
                    case "id":
                        builder.Id = reader.GetString();
                        break;
                    case "name":
                        builder.Name = reader.GetString();
                        break;
                    case "coll":
                        builder.Coll = reader.GetModule();

                        // if we encounter a mapped collection, jump to the class deserializer.
                        // NB this relies on the fact that docs on the wire always
                        // start with id and coll.
                        if (context.TryGetCollection(builder.Coll.Name, out var info))
                        {
                            if (info.ClassSerializer is IClassDocumentSerializer ser)
                            {
                                // This assumes ordering on the wire. If name is not null and we're here, then it's a named document so name is a string.
                                return ser.DeserializeDocument(context, builder.Id, builder.Name, ref reader);
                            }
                        }

                        if (builder.Id is not null)
                        {
                            return DocumentSerializer.Deserialize(builder, context, ref reader,
                                EndTokenFor(TokenType.StartDocument));
                        }

                        if (builder.Name is not null)
                        {
                            return NamedDocumentSerializer.Deserialize(builder, context, ref reader,
                                EndTokenFor(TokenType.StartDocument));
                        }

                        break;
                    default:
                        throw new SerializationException($"Unexpected field while deserializing @doc: {fieldName}");
                }
            }
            catch (NullDocumentException e)
            {
                return new NullDocument<object?>(e.Id, e.Name, e.Collection, e.Cause);
            }
        }

        throw new SerializationException("Unsupported document");
    }

    /// <summary>
    /// Serializes an object to a Fauna compatible format.
    /// </summary>
    /// <param name="ctx">The context for serialization.</param>
    /// <param name="w">The writer to serialize the object to.</param>
    /// <param name="o">The object to serialize.</param>
    /// <exception cref="SerializationException">Thrown when serialization fails.</exception>
    public override void Serialize(MappingContext ctx, Utf8FaunaWriter w, object? o)
    {
        if (o == null)
        {
            w.WriteNullValue();
            return;
        }

        var ser = Serializer.Generate(ctx, o.GetType());
        ser.Serialize(ctx, w, o);
    }
}
