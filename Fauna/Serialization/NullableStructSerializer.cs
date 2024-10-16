using Fauna.Mapping;

namespace Fauna.Serialization;

internal class NullableStructSerializer<T> : BaseSerializer<T?> where T : struct
{
    private readonly ISerializer<T> _inner;

    public NullableStructSerializer(ISerializer<T> inner)
    {
        _inner = inner;
    }

    public override List<FaunaType> GetSupportedTypes() => _inner.GetSupportedTypes();

    public override T? Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        return reader.CurrentTokenType == TokenType.Null ? new T?() : _inner.Deserialize(context, ref reader);
    }

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        _inner.Serialize(context, writer, (T?)o);
    }
}
