using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;


internal class StringSerializer : BaseSerializer<string?>
{
    public override string? Deserialize(MappingContext ctx, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.String => reader.GetString(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        throw new NotImplementedException();
    }
}

internal class ByteSerializer : BaseSerializer<byte>
{
    public override byte Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int => reader.GetByte(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        throw new NotImplementedException();
    }
}

internal class SByteSerializer : BaseSerializer<sbyte>
{
    public override sbyte Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int => reader.GetUnsignedByte(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        throw new NotImplementedException();
    }
}


internal class ShortSerializer : BaseSerializer<short>
{
    public override short Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetShort(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        throw new NotImplementedException();
    }
}

internal class UShortSerializer : BaseSerializer<ushort>
{
    public override ushort Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetUnsignedShort(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        throw new NotImplementedException();
    }
}

internal class IntSerializer : BaseSerializer<int>
{
    public override int Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetInt(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        throw new NotImplementedException();
    }
}

internal class UIntSerializer : BaseSerializer<uint>
{
    public override uint Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetUnsignedInt(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        throw new NotImplementedException();
    }
}

internal class LongSerializer : BaseSerializer<long>
{
    public override long Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetLong(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        throw new NotImplementedException();
    }
}

internal class FloatSerializer : BaseSerializer<float>
{
    public override float Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long or TokenType.Double => reader.GetFloat(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        throw new NotImplementedException();
    }
}

internal class DoubleSerializer : BaseSerializer<double>
{
    public override double Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long or TokenType.Double => reader.GetDouble(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        throw new NotImplementedException();
    }
}

internal class BooleanSerializer : BaseSerializer<bool>
{
    public override bool Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.True or TokenType.False => reader.GetBoolean(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        throw new NotImplementedException();
    }
}

internal class DateOnlySerializer : BaseSerializer<DateOnly>
{
    public override DateOnly Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Date => reader.GetDate(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        throw new NotImplementedException();
    }
}

internal class DateTimeSerializer : BaseSerializer<DateTime>
{
    public override DateTime Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Time => reader.GetTime(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        throw new NotImplementedException();
    }
}
