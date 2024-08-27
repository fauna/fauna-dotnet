using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization.Serializers;

internal class DynamicSerializer : BaseSerializer<object?>
{
    public static DynamicSerializer Singleton { get; } = new();

    private readonly ListSerializer<object?> _list;
    private readonly PageSerializer<object?> _page;
    private readonly DictionarySerializer<object?> _dict;
    private readonly DocumentSerializer<object> _doc;
    private readonly DocumentSerializer<object> _ref;

    private DynamicSerializer()
    {
        _list = new ListSerializer<object?>(this);
        _page = new PageSerializer<object?>(this);
        _dict = new DictionarySerializer<object?>(this);
        _doc = new DocumentSerializer<object>();
        _ref = new DocumentSerializer<object>();
    }

    public override object? Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.StartObject => _dict.Deserialize(context, ref reader),
            TokenType.StartArray => _list.Deserialize(context, ref reader),
            TokenType.StartPage => _page.Deserialize(context, ref reader),
            TokenType.StartRef => _ref.Deserialize(context, ref reader),
            TokenType.StartDocument => _doc.Deserialize(context, ref reader),
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

    /// <summary>
    /// Serializes an object to a Fauna compatible format.
    /// </summary>
    /// <param name="ctx">The context for serialization.</param>
    /// <param name="w">The writer to serialize the object to.</param>
    /// <param name="o">The object to serialize.</param>
    /// <exception cref="SerializationException">Thrown when serialization fails.</exception>
    public override void Serialize(MappingContext ctx, Utf8FaunaWriter w, object? o)
    {
        SerializeValueInternal(ctx, w, o);
    }

    private void SerializeValueInternal(MappingContext ctx, Utf8FaunaWriter w, object? o)
    {
        switch (o)
        {
            case null:
                w.WriteNullValue();
                break;
            case byte v:
                w.WriteIntValue(v);
                break;
            case sbyte v:
                w.WriteIntValue(v);
                break;
            case ushort v:
                w.WriteIntValue(v);
                break;
            case short v:
                w.WriteIntValue(v);
                break;
            case int v:
                w.WriteIntValue(v);
                break;
            case uint v:
                w.WriteLongValue(v);
                break;
            case long v:
                w.WriteLongValue(v);
                break;
            case float v:
                w.WriteDoubleValue(v);
                break;
            case double v:
                w.WriteDoubleValue(v);
                break;
            case decimal:
                throw new SerializationException("Decimals are unsupported due to potential loss of precision.");
            case bool v:
                w.WriteBooleanValue(v);
                break;
            case string v:
                w.WriteStringValue(v);
                break;
            case Module v:
                w.WriteModuleValue(v);
                break;
            case DateTime v:
                w.WriteTimeValue(v);
                break;
            case DateTimeOffset v:
                w.WriteTimeValue(v);
                break;
            case DateOnly v:
                w.WriteDateValue(v);
                break;
            case Ref doc:
                SerializeDocumentRefInternal(w, doc.Id, doc.Collection);
                break;
            case Document doc:
                SerializeDocumentRefInternal(w, doc.Id, doc.Collection);
                break;
            case NullableDocument<Document> doc:
                switch (doc)
                {
                    case NullDocument<Document> d:
                        SerializeDocumentRefInternal(w, d.Id, d.Collection);
                        break;
                    case NonNullDocument<Document> d:
                        SerializeDocumentRefInternal(w, d.Value!.Id, d.Value!.Collection);
                        break;
                }
                break;
            case NullableDocument<Ref> doc:
                switch (doc)
                {
                    case NullDocument<Ref> d:
                        SerializeDocumentRefInternal(w, d.Id, d.Collection);
                        break;
                    case NonNullDocument<Ref> d:
                        SerializeDocumentRefInternal(w, d.Value!.Id, d.Value!.Collection);
                        break;
                }
                break;
            case NamedRef doc:
                SerializeNamedDocumentRefInternal(w, doc.Name, doc.Collection);
                break;
            case NamedDocument doc:
                SerializeNamedDocumentRefInternal(w, doc.Name, doc.Collection);
                break;
            case NullableDocument<NamedDocument> doc:
                switch (doc)
                {
                    case NullDocument<NamedDocument> d:
                        SerializeNamedDocumentRefInternal(w, d.Id, d.Collection);
                        break;
                    case NonNullDocument<NamedDocument> d:
                        SerializeNamedDocumentRefInternal(w, d.Value!.Name, d.Value!.Collection);
                        break;
                }
                break;
            case NullableDocument<NamedRef> doc:
                switch (doc)
                {
                    case NullDocument<NamedRef> d:
                        SerializeNamedDocumentRefInternal(w, d.Id, d.Collection);
                        break;
                    case NonNullDocument<NamedRef> d:
                        SerializeNamedDocumentRefInternal(w, d.Value!.Name, d.Value!.Collection);
                        break;
                }
                break;
            case Dictionary<string, object> d:
                SerializeIDictionaryInternal(ctx, w, d);
                break;
            case IEnumerable<object> e:
                w.WriteStartArray();
                foreach (var obj in e)
                {
                    SerializeValueInternal(ctx, w, obj);
                }
                w.WriteEndArray();
                break;
            default:
                SerializeClassInternal(ctx, w, o);
                break;
        }
    }


    private void SerializeDocumentRefInternal(Utf8FaunaWriter writer, string id, Module coll)
    {
        writer.WriteStartRef();
        writer.WriteString("id", id);
        writer.WriteModule("coll", coll);
        writer.WriteEndRef();
    }

    private void SerializeNamedDocumentRefInternal(Utf8FaunaWriter writer, string name, Module coll)
    {
        writer.WriteStartRef();
        writer.WriteString("name", name);
        writer.WriteModule("coll", coll);
        writer.WriteEndRef();
    }


    private void SerializeIDictionaryInternal<T>(MappingContext ctx, Utf8FaunaWriter writer, IDictionary<string, T> d)
    {
        var shouldEscape = Serializer.Tags.Overlaps(d.Keys);
        if (shouldEscape) writer.WriteStartEscapedObject(); else writer.WriteStartObject();
        foreach (var (key, value) in d)
        {
            writer.WriteFieldName(key);
            Serialize(ctx, writer, value);
        }
        if (shouldEscape) writer.WriteEndEscapedObject(); else writer.WriteEndObject();
    }

    private void SerializeClassInternal(MappingContext ctx, Utf8FaunaWriter writer, object obj)
    {
        var t = obj.GetType();
        var mapping = ctx.GetInfo(t);
        var shouldEscape = mapping.ShouldEscapeObject;

        if (shouldEscape) writer.WriteStartEscapedObject(); else writer.WriteStartObject();
        foreach (var field in mapping.Fields)
        {
            writer.WriteFieldName(field.Name!);
            var v = field.Property.GetValue(obj);
            SerializeValueInternal(ctx, writer, v);
        }
        if (shouldEscape) writer.WriteEndEscapedObject(); else writer.WriteEndObject();
    }
}
