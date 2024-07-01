using Fauna.Mapping;

namespace Fauna.Serialization;

internal class LongDeserializer : BaseDeserializer<long>
{
    public override long Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int => reader.GetInt(),
            TokenType.Long => reader.GetLong(),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing: {reader.CurrentTokenType}"),
        };
}

internal class ShortDeserializer : BaseDeserializer<short>
{
    public override short Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int => reader.GetShort(),
            TokenType.Long => reader.GetShort(),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing: {reader.CurrentTokenType}"),
        };
}

internal class UShortDeserializer : BaseDeserializer<ushort>
{
    public override ushort Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int => reader.GetUnsignedShort(),
            TokenType.Long => reader.GetUnsignedShort(),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing: {reader.CurrentTokenType}"),
        };
}
