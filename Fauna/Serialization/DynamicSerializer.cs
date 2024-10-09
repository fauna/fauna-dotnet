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
    private readonly BaseRefSerializer<Dictionary<string, object>> _docref;


    private DynamicSerializer()
    {
        _list = new ListSerializer<object?>(this);
        _page = new PageSerializer<object?>(this);
        _dict = new DictionarySerializer<object?>(this);
        _docref = new BaseRefSerializer<Dictionary<string, object>>(_dict);
    }

    public override object? Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.StartObject => _dict.Deserialize(context, ref reader),
            TokenType.StartArray => _list.Deserialize(context, ref reader),
            TokenType.StartPage => _page.Deserialize(context, ref reader),
            TokenType.StartRef => _docref.Deserialize(context, ref reader),
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

    private object DeserializeDocumentInternal(MappingContext context, ref Utf8FaunaReader reader)
    {
        var builder = new BaseRefBuilder<Dictionary<string, object>>();
        while (reader.Read() && reader.CurrentTokenType != TokenType.EndDocument)
        {
            if (reader.CurrentTokenType != TokenType.FieldName)
                throw new SerializationException(
                    $"Unexpected token while deserializing @doc: {reader.CurrentTokenType}");

            string fieldName = reader.GetString()!;

            reader.Read();

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
                    builder.Collection = reader.GetModule();

                    // if we encounter a mapped collection, jump to the class deserializer.
                    // NB this relies on the fact that docs on the wire always start with id and coll.
                    if (context.TryGetCollection(builder.Collection.Name, out var info) && info.ClassSerializer is IPartialDocumentSerializer ser)
                    {
                        return new BaseRefBuilder<object>
                        {
                            Id = builder.Id,
                            Name = builder.Name,
                            Collection = builder.Collection,
                            Doc = ser.DeserializeDocument(context, builder.Id, builder.Name, builder.Collection,
                                ref reader)
                        }.Build();
                    }

                    builder.Doc = (Dictionary<string, object>?)_dict.DeserializeDocument(context, builder.Id, builder.Name, builder.Collection, ref reader);
                    break;
            }
        }

        return builder.Build();
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
