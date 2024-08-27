using Fauna.Exceptions;
using Fauna.Mapping;

namespace Fauna.Serialization.Serializers;

internal class CheckedSerializer<T> : BaseSerializer<T>
{
    public override T Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        var obj = DynamicSerializer.Singleton.Deserialize(context, ref reader);

        if (obj is T v) return v;

        throw new SerializationException(
            $"Expected type {typeof(T)} but received {obj?.GetType()}");
    }

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        DynamicSerializer.Singleton.Serialize(context, writer, o);
    }
}
