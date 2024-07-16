using Fauna.Mapping;

namespace Fauna.Serialization;

internal class NullableCodec<T> : BaseCodec<T?>
{
    private readonly ICodec<T> _inner;

    public NullableCodec(ICodec<T> inner)
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

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, T? o)
    {
        throw new NotImplementedException();
    }
}
