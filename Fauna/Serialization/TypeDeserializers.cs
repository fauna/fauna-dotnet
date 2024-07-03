using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;


internal class StringDeserializer : BaseDeserializer<string?>
{
    public override string? Deserialize(MappingContext ctx, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.String => reader.GetString(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };
}

internal class ByteDeserializer : BaseDeserializer<byte>
{
    public override byte Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int => reader.GetByte(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };
}

internal class SByteDeserializer : BaseDeserializer<sbyte>
{
    public override sbyte Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int => reader.GetUnsignedByte(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };
}


internal class ShortDeserializer : BaseDeserializer<short>
{
    public override short Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetShort(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };
}

internal class UShortDeserializer : BaseDeserializer<ushort>
{
    public override ushort Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetUnsignedShort(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };
}

internal class IntDeserializer : BaseDeserializer<int>
{
    public override int Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetInt(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };
}

internal class UIntDeserializer : BaseDeserializer<uint>
{
    public override uint Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetUnsignedInt(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };
}

internal class LongDeserializer : BaseDeserializer<long>
{
    public override long Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetLong(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };
}

internal class FloatDeserializer : BaseDeserializer<float>
{
    public override float Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long or TokenType.Double => reader.GetFloat(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };
}

internal class DoubleDeserializer : BaseDeserializer<double>
{
    public override double Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long or TokenType.Double => reader.GetDouble(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };
}

internal class BooleanDeserializer : BaseDeserializer<bool>
{
    public override bool Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.True or TokenType.False => reader.GetBoolean(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };
}

internal class DateOnlyDeserializer : BaseDeserializer<DateOnly>
{
    public override DateOnly Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Date => reader.GetDate(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };
}

internal class DateTimeDeserializer : BaseDeserializer<DateTime>
{
    public override DateTime Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Time => reader.GetTime(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };
}

internal class ModuleDeserializer : BaseDeserializer<Module>
{
    public override Module Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Module => reader.GetModule(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };
}
