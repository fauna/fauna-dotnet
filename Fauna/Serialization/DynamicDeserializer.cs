using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal class DynamicDeserializer : BaseDeserializer<object?>
{
    public static DynamicDeserializer Singleton { get; } = new();

    private readonly ListDeserializer<object?> _list;
    private readonly PageDeserializer<object?> _page;
    private readonly DictionaryDeserializer<object?> _dict;

    private DynamicDeserializer()
    {
        _list = new ListDeserializer<object?>(this);
        _page = new PageDeserializer<object?>(this);
        _dict = new DictionaryDeserializer<object?>(this);
    }

    public override object? Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.StartObject => _dict.Deserialize(context, ref reader),
            TokenType.StartArray => _list.Deserialize(context, ref reader),
            TokenType.StartPage => _page.Deserialize(context, ref reader),
            TokenType.StartRef or TokenType.StartDocument => DeserializeDocumentOrRef(context, ref reader, reader.CurrentTokenType),
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

    private object DeserializeDocumentOrRef(MappingContext context, ref Utf8FaunaReader reader, TokenType startToken)
    {
        var data = new Dictionary<string, object?>();
        string? id = null;
        string? name = null;
        DateTime? ts = null;
        Module? coll = null;
        string? cause = null;
        bool? exists = null;


        var endToken = startToken == TokenType.StartRef ? TokenType.EndRef : TokenType.EndDocument;

        while (reader.Read() && reader.CurrentTokenType != endToken)
        {
            if (reader.CurrentTokenType != TokenType.FieldName)
                throw new SerializationException(
                    $"Unexpected token while deserializing into Document: {reader.CurrentTokenType}");

            var fieldName = reader.GetString()!;
            reader.Read();
            switch (fieldName)
            {
                case "id":
                    id = reader.GetString();
                    break;
                case "name":
                    name = reader.GetString();
                    break;
                case "coll":
                    coll = reader.GetModule();

                    // if we encounter a mapped collection, jump to the class deserializer.
                    // NB this relies on the fact that docs on the wire always
                    // start with id and coll.
                    if (context.TryGetCollection(coll.Name, out var collInfo))
                    {
                        return collInfo.Deserializer.DeserializeDocument(context, id, name, ref reader);
                    }

                    break;
                case "ts":
                    ts = reader.GetTime();
                    break;
                case "exists":
                    exists = reader.GetBoolean();
                    break;
                case "cause":
                    cause = reader.GetString();
                    break;
                default:
                    data[fieldName] = Singleton.Deserialize(context, ref reader);
                    break;
            }
        }

        if (id != null && coll != null && ts != null && startToken == TokenType.StartDocument)
        {
            if (name != null) data["name"] = name;
            if (cause != null) data["cause"] = cause;
            if (exists != null) data["exists"] = exists;
            return new Document(id, coll, ts, data);
        }

        if (name != null && coll != null && ts != null && startToken == TokenType.StartDocument)
        {
            return new NamedDocument(name, coll, ts, data);
        }

        if (id != null && coll != null && startToken == TokenType.StartRef)
        {
            if (exists != null && !exists.Value)
            {
                return new Document(id, coll, ts, data, cause);
            }

            return new DocumentRef(id, coll, cause);
        }

        if (name != null && coll != null && startToken == TokenType.StartRef)
        {
            if (exists != null && !exists.Value)
            {
                return new NamedDocument(name, coll, ts, data, cause);
            }

            return new NamedDocumentRef(name, coll, cause);
        }


        // Unsupported document type, but don't fail for forward compatibility.
        if (id != null) data["id"] = id;
        if (name != null) data["name"] = name;
        if (coll != null) data["coll"] = coll;
        if (ts != null) data["ts"] = ts;
        return data;
    }
}
