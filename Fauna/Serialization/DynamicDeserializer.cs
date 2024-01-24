using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal class DynamicDeserializer : BaseDeserializer<object?>
{
    public static readonly DynamicDeserializer Singleton = new();

    private readonly ListDeserializer<object?> _list;
    private readonly PageDeserializer<object?> _page;
    private readonly DictionaryDeserializer<object?> _dict;

    private DynamicDeserializer()
    {
        _list = new ListDeserializer<object?>(this);
        _page = new PageDeserializer<object?>(this);
        _dict = new DictionaryDeserializer<object?>(this);
    }

    public override object? Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        var value = reader.CurrentTokenType switch
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

        return value;
    }

    private object DeserializeRef(MappingContext context, ref Utf8FaunaReader reader)
    {
        string? id = null;
        string? name = null;
        Module? coll = null;
        var exists = true;
        string? cause = null;
        var allProps = new Dictionary<string, object?>();

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
                    allProps["id"] = id;
                    break;
                case "name":
                    name = reader.GetString();
                    allProps["name"] = name;
                    break;
                case "coll":
                    coll = reader.GetModule();
                    allProps["coll"] = coll;
                    break;
                case "exists":
                    exists = reader.GetBoolean();
                    allProps["exists"] = exists;
                    break;
                case "cause":
                    cause = reader.GetString();
                    allProps["cause"] = cause;
                    break;
                default:
                    allProps[fieldName] = DynamicDeserializer.Singleton.Deserialize(context, ref reader);
                    break;
            }
        }

        if (id != null && coll != null)
        {
            if (exists)
            {
                return new DocumentRef(id, coll);
            }

            return new NullDocumentRef(id, coll, cause!);
        }

        if (name != null && coll != null)
        {
            if (exists)
            {
                return new NamedDocumentRef(name, coll);
            }

            return new NullNamedDocumentRef(name, coll, cause!);
        }

        // Unsupported ref type, but don't fail for forward compatibility.
        return allProps;
    }

    private object DeserializeDocument(MappingContext context, ref Utf8FaunaReader reader)
    {
        var data = new Dictionary<string, object?>();
        string? id = null;
        string? name = null;
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
                    name = reader.GetString();
                    break;
                case "ts":
                    ts = reader.GetTime();
                    break;
                case "coll":
                    coll = reader.GetModule();
                    break;
                default:
                    data[fieldName] = DynamicDeserializer.Singleton.Deserialize(context, ref reader);
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
            return new NamedDocument(name, coll, ts.GetValueOrDefault(), data);
        }

        // Unsupported document type, but don't fail for forward compatibility.
        if (id != null) data["id"] = id;
        if (name != null) data["name"] = name;
        if (coll != null) data["coll"] = coll;
        if (ts != null) data["ts"] = ts;
        return data;
    }
}
