using Fauna.Types;

namespace Fauna.Serialization;

internal class PageDeserializer<T> : BaseDeserializer<Page<T>>
{
    private IDeserializer<List<T>> _dataDeserializer;

    public PageDeserializer(IDeserializer<T> elemDeserializer)
    {
        _dataDeserializer = new ListDeserializer<T>(elemDeserializer);
    }

    public override Page<T> Deserialize(SerializationContext context, ref Utf8FaunaReader reader)
    {
        if (reader.CurrentTokenType != TokenType.StartPage)
            throw new SerializationException(
                $"Unexpected token while deserializing into {typeof(Page<T>)}: {reader.CurrentTokenType}");

        List<T>? data = null;
        string? after = null;

        while (reader.Read() && reader.CurrentTokenType != TokenType.EndPage)
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

        if (data is null)
            throw new SerializationException($"No page data found while deserializing into {typeof(Page<T>)}");

        return new Page<T>(data!, after);
    }
}
