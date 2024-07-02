using Fauna.Mapping;

namespace Fauna.Serialization;


internal class ByteDeserializer : BaseDeserializer<byte>
{
    public override byte Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int => reader.GetByte(),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing: {reader.CurrentTokenType}"),
        };
}

internal class SByteDeserializer : BaseDeserializer<sbyte>
{
    public override sbyte Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int => reader.GetUnsignedByte(),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing: {reader.CurrentTokenType}"),
        };
}


internal class ShortDeserializer : BaseDeserializer<short>
{
    public override short Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetShort(),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing: {reader.CurrentTokenType}"),
        };
}

internal class UShortDeserializer : BaseDeserializer<ushort>
{
    public override ushort Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetUnsignedShort(),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing: {reader.CurrentTokenType}"),
        };
}

internal class IntDeserializer : BaseDeserializer<int>
{
    public override int Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetInt(),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing: {reader.CurrentTokenType}"),
        };
}

internal class UIntDeserializer : BaseDeserializer<uint>
{
    public override uint Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetUnsignedInt(),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing: {reader.CurrentTokenType}"),
        };
}

internal class LongDeserializer : BaseDeserializer<long>
{
    public override long Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetLong(),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing: {reader.CurrentTokenType}"),
        };
}

internal class FloatDeserializer : BaseDeserializer<float>
{
    public override float Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long or TokenType.Double => reader.GetFloat(),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing: {reader.CurrentTokenType}"),
        };
}

internal class DoubleDeserializer : BaseDeserializer<double>
{
    public override double Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long or TokenType.Double => reader.GetDouble(),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing: {reader.CurrentTokenType}"),
        };
}
