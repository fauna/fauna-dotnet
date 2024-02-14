using Fauna.Mapping;

namespace Fauna.Serialization;

internal class NullableDeserializer<T> : BaseDeserializer<T?>
{
    private readonly IDeserializer<T> _inner;

    public NullableDeserializer(IDeserializer<T> inner)
    {
        _inner = inner;
    }

    public override T? Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        if (reader.CurrentTokenType == TokenType.Null)
        {
            return default(T);
        }

        return _inner.Deserialize(context, ref reader);
    }
}
