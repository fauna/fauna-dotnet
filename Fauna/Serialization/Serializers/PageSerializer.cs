using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization.Serializers;

internal class PageSerializer<T> : BaseSerializer<Page<T>>
{
    private readonly ISerializer<List<T>> _dataSerializer;

    public PageSerializer(ISerializer<T> elemSerializer)
    {
        _dataSerializer = new ListSerializer<T>(elemSerializer);
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
            data = _dataSerializer.Deserialize(context, ref reader);
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
                        data = _dataSerializer.Deserialize(context, ref reader);
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

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        DynamicSerializer.Singleton.Serialize(context, writer, o);
    }
}
