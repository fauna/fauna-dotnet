using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;


internal class StringCodec : BaseCodec<string?>
{
    public override string? Deserialize(MappingContext ctx, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.String => reader.GetString(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, string? o)
    {
        writer.WriteStringValue(o!);
    }
}

internal class ByteCodec : BaseCodec<byte>
{
    public override byte Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int => reader.GetByte(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, byte o)
    {
        throw new NotImplementedException();
    }
}

internal class SByteCodec : BaseCodec<sbyte>
{
    public override sbyte Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int => reader.GetUnsignedByte(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, sbyte o)
    {
        throw new NotImplementedException();
    }
}


internal class ShortCodec : BaseCodec<short>
{
    public override short Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetShort(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, short o)
    {
        throw new NotImplementedException();
    }
}

internal class UShortCodec : BaseCodec<ushort>
{
    public override ushort Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetUnsignedShort(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, ushort o)
    {
        throw new NotImplementedException();
    }
}

internal class IntCodec : BaseCodec<int>
{
    public override int Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetInt(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, int o)
    {
        throw new NotImplementedException();
    }
}

internal class UIntCodec : BaseCodec<uint>
{
    public override uint Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetUnsignedInt(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, uint o)
    {
        throw new NotImplementedException();
    }
}

internal class LongCodec : BaseCodec<long>
{
    public override long Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetLong(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, long o)
    {
        throw new NotImplementedException();
    }
}

internal class FloatCodec : BaseCodec<float>
{
    public override float Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long or TokenType.Double => reader.GetFloat(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, float o)
    {
        throw new NotImplementedException();
    }
}

internal class DoubleCodec : BaseCodec<double>
{
    public override double Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long or TokenType.Double => reader.GetDouble(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, double o)
    {
        throw new NotImplementedException();
    }
}

internal class BooleanCodec : BaseCodec<bool>
{
    public override bool Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.True or TokenType.False => reader.GetBoolean(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, bool o)
    {
        throw new NotImplementedException();
    }
}

internal class DateOnlyCodec : BaseCodec<DateOnly>
{
    public override DateOnly Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Date => reader.GetDate(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, DateOnly o)
    {
        throw new NotImplementedException();
    }
}

internal class DateTimeCodec : BaseCodec<DateTime>
{
    public override DateTime Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Time => reader.GetTime(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, DateTime o)
    {
        throw new NotImplementedException();
    }
}

internal class ModuleCodec : BaseCodec<Module>
{
    public override Module Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Module => reader.GetModule(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, ref Utf8FaunaWriter writer, Module? o)
    {
        throw new NotImplementedException();
    }
}
