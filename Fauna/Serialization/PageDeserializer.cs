using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal interface PageDeserializer
{
    public ListDeserializer Data { get; }
    public IDeserializer Elem { get; }

}

internal class PageDeserializer<T> : BaseDeserializer<Page<T>>, PageDeserializer
{
    public ListDeserializer<T> Data { get; }
    public IDeserializer<T> Elem { get => Data.Elem; }
    ListDeserializer PageDeserializer.Data { get => Data; }
    IDeserializer PageDeserializer.Elem { get => Elem; }

    public PageDeserializer(IDeserializer<T> elem)
    {
        Data = new ListDeserializer<T>(elem);
    }

    public override Page<T> Deserialize(MappingContext context, ref Utf8FaunaReader reader)
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
                    data = Data.Deserialize(context, ref reader);
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
