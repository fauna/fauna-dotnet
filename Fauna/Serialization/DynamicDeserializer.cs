using Fauna.Exceptions;
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
            TokenType.StartRef => DeserializeRef(context, ref reader),
            TokenType.StartDocument => DeserializeDocument(context, ref reader),
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

    private object DeserializeRef(MappingContext context, ref Utf8FaunaReader reader)
    {
        string? id = null;
        string? name = null;
        Module? coll = null;
        string? cause = null;
        var exists = true;

        while (reader.Read() && reader.CurrentTokenType != TokenType.EndRef)
        {
            if (reader.CurrentTokenType != TokenType.FieldName)
                throw new SerializationException(
                    $"Unexpected token while deserializing into DocumentRef: {reader.CurrentTokenType}");

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
                    break;
                case "cause":
                    cause = reader.GetString();
                    break;
                case "exists":
                    exists = reader.GetBoolean();
                    break;
            }
        }

        if (id != null && coll != null && exists)
        {
            return new DocumentRef(id, coll);
        }

        if (name != null && coll != null && exists)
        {
            return new NamedDocumentRef(name, coll);
        }

        if ((id != null || name != null) && coll != null && !exists)
        {
            throw new NullDocumentException(
                $"Document {id ?? name} in collection {coll.Name} is null: {cause}",
                (id ?? name)!,
                coll,
                cause!);
        }

        throw new SerializationException("Unsupported reference type");
    }

    private object DeserializeDocument(MappingContext context, ref Utf8FaunaReader reader)
    {
        var data = new Dictionary<string, object?>();
        string? id = null;
        object? name = null;
        DateTime? ts = null;
        Module? coll = null;

        while (reader.Read() && reader.CurrentTokenType != TokenType.EndDocument)
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
                    name = Singleton.Deserialize(context, ref reader);
                    break;
                case "coll":
                    coll = reader.GetModule();

                    // if we encounter a mapped collection, jump to the class deserializer.
                    // NB this relies on the fact that docs on the wire always
                    // start with id and coll.
                    if (context.TryGetCollection(coll.Name, out var collInfo))
                    {
                        // This assumes ordering on the wire. If name is not null and we're here, then it's a named document so name is a string.
                        return collInfo.Deserializer.DeserializeDocument(context, id, name != null ? (string)name : null, ref reader);
                    }

                    break;
                case "ts":
                    ts = reader.GetTime();
                    break;
                default:
                    data[fieldName] = Singleton.Deserialize(context, ref reader);
                    break;
            }
        }

        if (id != null && coll != null && ts != null)
        {
            if (name != null) data["name"] = name;
            return new Document(id, coll, ts.GetValueOrDefault(), data);
        }

        if (name != null && coll != null && ts != null)
        {
            // If we're here, name is a string.
            return new NamedDocument((string)name, coll, ts.GetValueOrDefault(), data);
        }

        // Unsupported document type, but don't fail for forward compatibility.
        if (id != null) data["id"] = id;
        if (name != null) data["name"] = name;
        if (coll != null) data["coll"] = coll;
        if (ts != null) data["ts"] = ts;
        return data;
    }
}
