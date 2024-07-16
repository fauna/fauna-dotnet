using Fauna.Mapping;

namespace Fauna.Serialization;

internal class CheckedCodec<T> : BaseCodec<T>
{
    public override T Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        var obj = DynamicCodec.Singleton.Deserialize(context, ref reader);

        if (obj is T v) return v;

        throw new SerializationException(
            $"Expected type {typeof(T)} but received {obj?.GetType()}");
    }

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, T? o)
    {
        throw new NotImplementedException();
    }
}
