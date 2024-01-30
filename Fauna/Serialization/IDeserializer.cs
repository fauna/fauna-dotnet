using Fauna.Mapping;

namespace Fauna.Serialization;

public interface IDeserializer<out T> : IDeserializer
{
    new T Deserialize(MappingContext context, ref Utf8FaunaReader reader);
}

public interface IDeserializer
{
    object? Deserialize(MappingContext context, ref Utf8FaunaReader reader);
}

public abstract class BaseDeserializer<T> : IDeserializer<T>
{
    object? IDeserializer.Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        Deserialize(context, ref reader);

    public abstract T Deserialize(MappingContext context, ref Utf8FaunaReader reader);
}
