using Fauna.Mapping;

namespace Fauna.Serialization;

internal class NullableSerializer<T> : BaseSerializer<T?>
{
    private readonly ISerializer<T> _inner;

    public NullableSerializer(ISerializer<T> inner)
    {
        _inner = inner;
    }

    public override List<FaunaType> GetSupportedTypes() => _inner.GetSupportedTypes();

    public override T? Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        if (reader.CurrentTokenType == TokenType.Null)
        {
            return default(T);
        }

        return _inner.Deserialize(context, ref reader);
    }

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        _inner.Serialize(context, writer, (T?)o);
    }
}
