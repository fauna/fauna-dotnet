using Fauna.Mapping;

namespace Fauna.Serialization;

internal class CheckedDeserializer<T> : BaseDeserializer<T>
{
    public override T Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        var obj = DynamicDeserializer.Singleton.Deserialize(context, ref reader);

        if (obj is T v) return v;

        throw new SerializationException(
            $"Expected type {typeof(T)} but received {obj?.GetType()}");
    }
}
