using Fauna.Mapping;

namespace Fauna.Serialization;

internal class NullableStructCodec<T> : BaseCodec<T?> where T : struct
{
    private readonly ICodec<T> _inner;

    public NullableStructCodec(ICodec<T> inner)
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

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, T? o)
    {
        throw new NotImplementedException();
    }
}
