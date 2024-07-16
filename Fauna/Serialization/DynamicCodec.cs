using Fauna.Mapping;

namespace Fauna.Serialization;

internal class DynamicCodec : BaseCodec<object?>
{
    public static DynamicCodec Singleton { get; } = new();

    private readonly ListCodec<object?> _list;
    private readonly PageCodec<object?> _page;
    private readonly DictionaryCodec<object?> _dict;
    private readonly DocumentCodec<object> _doc;
    private readonly DocumentCodec<object> _ref;

    private DynamicCodec()
    {
        _list = new ListCodec<object?>(this);
        _page = new PageCodec<object?>(this);
        _dict = new DictionaryCodec<object?>(this);
        _doc = new DocumentCodec<object>();
        _ref = new DocumentCodec<object>();
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

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, object? o)
    {
        throw new NotImplementedException();
    }
}
