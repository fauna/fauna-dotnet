using Fauna.Mapping;

namespace Fauna.Serialization.Serializers;

internal class NullableStructSerializer<T> : BaseSerializer<T?> where T : struct
{
    private readonly ISerializer<T> _inner;

    public NullableStructSerializer(ISerializer<T> inner)
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

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        DynamicSerializer.Singleton.Serialize(context, writer, o);
    }
}
