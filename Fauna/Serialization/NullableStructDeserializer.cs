using Fauna.Mapping;

namespace Fauna.Serialization;

internal class NullableStructDeserializer<T> : BaseDeserializer<T?> where T : struct
{
    private readonly IDeserializer<T> _inner;

    public NullableStructDeserializer(IDeserializer<T> inner)
    {
        _inner = inner;
    }

    public override T? Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        if (reader.CurrentTokenType == TokenType.Null)
        {
            return new T?();
        }

        return _inner.Deserialize(context, ref reader);
    }
}
