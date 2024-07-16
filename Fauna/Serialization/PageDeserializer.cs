using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal class PageDeserializer<T> : BaseDeserializer<Page<T>>
{
    private readonly IDeserializer<List<T>> _dataDeserializer;

    public PageDeserializer(IDeserializer<T> elemDeserializer)
    {
        _dataDeserializer = new ListDeserializer<T>(elemDeserializer);
    }

    public override Page<T> Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        var wrapInPage = false;
        var endToken = TokenType.None;
        switch (reader.CurrentTokenType)
        {
            case TokenType.StartPage:
                endToken = TokenType.EndPage;
                break;
            case TokenType.StartObject:
                endToken = TokenType.EndObject;
                break;
            default:
                wrapInPage = true;
                break;
        }

        List<T>? data = null;
        string? after = null;

        if (wrapInPage)
        {
            data = _dataDeserializer.Deserialize(context, ref reader);
        }
        else
        {
            while (reader.Read() && reader.CurrentTokenType != endToken)
            {
                var fieldName = reader.GetString()!;
                reader.Read();

                switch (fieldName)
                {
                    case "data":
                        data = _dataDeserializer.Deserialize(context, ref reader);
                        break;
                    case "after":
                        after = reader.GetString()!;
                        break;
                }
            }
        }

        if (data is null)
            throw new SerializationException($"No page data found while deserializing into {typeof(Page<T>)}");

        return new Page<T>(data!, after);
    }
}
