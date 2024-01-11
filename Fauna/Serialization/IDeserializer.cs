namespace Fauna.Serialization;

public interface IDeserializer<T> : IDeserializer
{
    new T Deserialize(SerializationContext context, ref Utf8FaunaReader reader);
}

public interface IDeserializer
{
    object? Deserialize(SerializationContext context, ref Utf8FaunaReader reader);
}

public abstract class BaseDeserializer<T> : IDeserializer<T>
{
    object? IDeserializer.Deserialize(SerializationContext context, ref Utf8FaunaReader reader) =>
        Deserialize(context, ref reader);

    public abstract T Deserialize(SerializationContext context, ref Utf8FaunaReader reader);
}
