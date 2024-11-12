using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal class PageSerializer<T> : BaseSerializer<Page<T>>
{
    private readonly ISerializer<List<T>> _dataSerializer;

    public PageSerializer(ISerializer<T> elemSerializer)
    {
        _dataSerializer = new ListSerializer<T>(elemSerializer);
    }

    public override List<FaunaType> GetSupportedTypes() => [FaunaType.Null, FaunaType.Set];

    public override Page<T> Deserialize(MappingContext ctx, ref Utf8FaunaReader reader)
    {
        bool wrapInPage = false;
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

        if (wrapInPage)
        {
            var data = _dataSerializer.Deserialize(ctx, ref reader);
            return new Page<T>(data, null);
        }

        reader.Read();
        return reader.CurrentTokenType == TokenType.String
            ? HandleUnmaterialized(ctx, ref reader, endToken)
            : HandleMaterialized(ctx, ref reader, endToken);
    }

    private Page<T> HandleUnmaterialized(MappingContext ctx, ref Utf8FaunaReader reader, TokenType endToken)
    {
        string after = reader.GetString()!;
        reader.Read();
        if (reader.CurrentTokenType != endToken)
        {
            throw UnexpectedToken(reader.CurrentTokenType);
        }

        return new Page<T>([], after);
    }

    private Page<T> HandleMaterialized(MappingContext ctx, ref Utf8FaunaReader reader, TokenType endToken)
    {
        List<T> data = [];
        string? after = null;

        do
        {
            string fieldName = reader.GetString()!;
            reader.Read();

            switch (fieldName)
            {
                case "data":
                    data = _dataSerializer.Deserialize(ctx, ref reader);
                    break;
                case "after":
                    after = reader.GetString()!;
                    break;
            }
        } while (reader.Read() && reader.CurrentTokenType != endToken);

        return new Page<T>(data, after);
    }

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        throw new NotImplementedException();
    }
}
