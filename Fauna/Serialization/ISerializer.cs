using Fauna.Exceptions;
using Fauna.Mapping;

namespace Fauna.Serialization;

/// <summary>
/// A generic interface defining serialize and deserialize behavior, and for declaring which each supported <see cref="FaunaType"/>.
/// </summary>
/// <typeparam name="T">The type to which the <see cref="ISerializer{T}"/> applies.</typeparam>
public interface ISerializer<out T> : ISerializer
{
    /// <summary>
    /// Consumes all or some of a <see cref="Utf8FaunaReader"/> and returns an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="ctx">A <see cref="MappingContext"/> used to influence deserialization.</param>
    /// <param name="reader">The <see cref="Utf8FaunaReader"/> to consume.</param>
    /// <returns>An instance representing part or all of the consumed <see cref="Utf8FaunaReader"/>.</returns>
    new T Deserialize(MappingContext ctx, ref Utf8FaunaReader reader);
}

/// <summary>
/// An interface defining serialize and deserialize behavior, and for declaring which each supported <see cref="FaunaType"/>.
/// </summary>
public interface ISerializer
{
    /// <summary>
    /// Consumes all or some of a <see cref="Utf8FaunaReader"/> and returns an object or null.
    /// </summary>
    /// <param name="ctx">A <see cref="MappingContext"/> used to influence deserialization.</param>
    /// <param name="reader">The <see cref="Utf8FaunaReader"/> to consume.</param>
    /// <returns>An instance representing part or all of the consumed <see cref="Utf8FaunaReader"/>.</returns>
    object? Deserialize(MappingContext ctx, ref Utf8FaunaReader reader);

    /// <summary>
    /// Serializes the provided object into the <see cref="Utf8FaunaWriter"/>.
    /// </summary>
    /// <param name="ctx">>A <see cref="MappingContext"/> used to influence serialization.</param>
    /// <param name="writer">The <see cref="Utf8FaunaWriter"/> to write to.</param>
    /// <param name="o">The object to serialize.</param>
    void Serialize(MappingContext ctx, Utf8FaunaWriter writer, object? o);

    /// <summary>
    /// A list of types to which the <see cref="ISerializer"/> applies.
    /// </summary>
    /// <returns>A list of <see cref="FaunaType"/>.</returns>
    List<FaunaType> GetSupportedTypes();
}

/// <summary>
/// An abstract class encapsulating common serialization and deserialization logic.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseSerializer<T> : ISerializer<T>
{
    /// <summary>
    /// Supported types for the serializer.
    /// </summary>
    /// <returns>A list of supported types.</returns>
    public virtual List<FaunaType> GetSupportedTypes() => new List<FaunaType>();

    /// <summary>
    /// Gets the end token for a given start token.
    /// </summary>
    /// <param name="start">A start token.</param>
    /// <returns>The end token.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the start token does not have a related end token.</exception>
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

    /// <summary>
    /// A helper to build an unsupported serialization type exception message.
    /// </summary>
    /// <param name="type">The unsupported type</param>
    /// <returns>An exception message to use.</returns>
    protected string UnsupportedSerializationTypeMessage(Type type) => $"Cannot serialize `{type}` with `{GetType()}`";

    /// <summary>
    /// A helper to build an unexpected type decoding exception message.
    /// </summary>
    /// <param name="faunaType">The unexpected fauna type.</param>
    /// <returns>An exception message to use.</returns>
    protected string UnexpectedTypeDecodingMessage(FaunaType faunaType) =>
        $"Unable to deserialize `{faunaType.GetType().Name}.{faunaType}` with `{GetType().Name}`. " +
        $"Supported types are [{string.Join(", ", GetSupportedTypes().Select(x => $"{x.GetType().Name}.{x}"))}]";

    object? ISerializer.Deserialize(MappingContext ctx, ref Utf8FaunaReader reader) =>
        Deserialize(ctx, ref reader);

    /// <summary>
    /// Consumes or partially consumes the provided reader and deserializes into a result.
    /// </summary>
    /// <param name="ctx">A <see cref="MappingContext"/> to influence deserialization.</param>
    /// <param name="reader">A <see cref="Utf8FaunaReader"/> to consume or partially consume.</param>
    /// <returns></returns>
    public abstract T Deserialize(MappingContext ctx, ref Utf8FaunaReader reader);

    void ISerializer.Serialize(MappingContext context, Utf8FaunaWriter writer, object? o) =>
        Serialize(context, writer, o);

    /// <summary>
    /// Serializes the provided object onto the <see cref="Utf8FaunaWriter"/>
    /// </summary>
    /// <param name="context">A <see cref="MappingContext"/> to influence serialization.</param>
    /// <param name="writer">A <see cref="Utf8FaunaWriter"/> to write to.</param>
    /// <param name="o">The object to write.</param>
    public abstract void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o);

    /// <summary>
    /// Creates a serailization exception with an unexpected token exception message.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    protected static SerializationException UnexpectedToken(TokenType token) =>
        new($"Unexpected token while deserializing: {token}");
}
