using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal class PageCodec<T> : BaseCodec<Page<T>>
{
    private readonly ICodec<List<T>> _dataCodec;

    public PageCodec(ICodec<T> elemCodec)
    {
        _dataCodec = new ListCodec<T>(elemCodec);
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
            data = _dataCodec.Deserialize(context, ref reader);
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
                        data = _dataCodec.Deserialize(context, ref reader);
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

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, Page<T>? o)
    {
        throw new NotImplementedException();
    }
}
