using Fauna.Exceptions;
using Fauna.Mapping;

namespace Fauna.Serialization;

public interface ISerializer<out T> : ISerializer
{
    new T Deserialize(MappingContext context, ref Utf8FaunaReader reader);
}

public interface ISerializer
{
    object? Deserialize(MappingContext context, ref Utf8FaunaReader reader);
    void Serialize(MappingContext ctx, Utf8FaunaWriter w, object? o);
    List<FaunaType> GetSupportedTypes();
}

public abstract class BaseSerializer<T> : ISerializer<T>
{
    public virtual List<FaunaType> GetSupportedTypes() => new List<FaunaType>();

    protected static TokenType EndTokenFor(TokenType start)
    {
        return start switch
        {
            TokenType.StartObject => TokenType.EndObject,
            TokenType.StartArray => TokenType.EndArray,
            TokenType.StartPage => TokenType.EndPage,
            TokenType.StartRef => TokenType.EndRef,
            TokenType.StartDocument => TokenType.EndDocument,
            _ => throw new ArgumentOutOfRangeException(nameof(start), start, null)
        };
    }

    protected string UnexpectedTokenExceptionMessage(TokenType token) => $"Unexpected token `{token}` deserializing with `{GetType().Name}`";

    protected string UnsupportedSerializationTypeMessage(Type type) => $"Cannot serialize `{type}` with `{GetType()}`";

    protected string UnexpectedTypeDecodingMessage(FaunaType faunaType) =>
        $"Unable to deserialize `{faunaType.GetType().Name}.{faunaType}` with `{GetType().Name}`. " +
        $"Supported types are [{string.Join(", ", GetSupportedTypes().Select(x => $"{x.GetType().Name}.{x}"))}]";

    object? ISerializer.Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        Deserialize(context, ref reader);

    public abstract T Deserialize(MappingContext context, ref Utf8FaunaReader reader);

    void ISerializer.Serialize(MappingContext context, Utf8FaunaWriter writer, object? o) =>
        Serialize(context, writer, o);

    public abstract void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o);

    protected static SerializationException UnexpectedToken(TokenType token) =>
        new($"Unexpected token while deserializing: {token}");
}
