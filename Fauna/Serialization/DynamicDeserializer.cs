using Fauna.Mapping;

namespace Fauna.Serialization;

internal class DynamicDeserializer : BaseDeserializer<object?>
{
    public static DynamicDeserializer Singleton { get; } = new();

    private readonly ListDeserializer<object?> _list;
    private readonly PageDeserializer<object?> _page;
    private readonly DictionaryDeserializer<object?> _dict;
    private readonly DocumentDeserializer<object> _doc;
    private readonly DocumentDeserializer<object> _ref;

    private DynamicDeserializer()
    {
        _list = new ListDeserializer<object?>(this);
        _page = new PageDeserializer<object?>(this);
        _dict = new DictionaryDeserializer<object?>(this);
        _doc = new DocumentDeserializer<object>();
        _ref = new DocumentDeserializer<object>();
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
}
