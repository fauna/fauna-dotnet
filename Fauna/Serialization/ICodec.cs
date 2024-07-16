using Fauna.Mapping;

namespace Fauna.Serialization;

public interface ICodec<out T> : ICodec
{
    new T Deserialize(MappingContext context, ref Utf8FaunaReader reader);
}

public interface ICodec
{
    object? Deserialize(MappingContext context, ref Utf8FaunaReader reader);
    void Serialize(MappingContext ctx, Utf8FaunaWriter w, object? o);
}

public abstract class BaseCodec<T> : ICodec<T>
{
    protected static readonly HashSet<string> Tags = new()
    {
        "@int", "@long", "@double", "@date", "@time", "@mod", "@ref", "@doc", "@set", "@object"
    };

    object? ICodec.Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        Deserialize(context, ref reader);

    void ICodec.Serialize(MappingContext context, Utf8FaunaWriter writer, object? o) =>
        Serialize(context, ref writer, (T?)o);

    public abstract T Deserialize(MappingContext context, ref Utf8FaunaReader reader);

    public abstract void Serialize(MappingContext context, ref Utf8FaunaWriter writer, T? o);

    protected static SerializationException UnexpectedToken(TokenType token) =>
        new($"Unexpected token while deserializing: {token}");
}
