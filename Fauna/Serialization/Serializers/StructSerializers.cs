using Fauna.Exceptions;
using Fauna.Mapping;

namespace Fauna.Serialization.Serializers;


internal class StringSerializer : BaseSerializer<string?>
{
    public override string? Deserialize(MappingContext ctx, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Null => null,
            TokenType.String => reader.GetString(),
            _ => throw new SerializationException(UnexpectedTokenExceptionMessage(reader.CurrentTokenType))
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case string s:
                writer.WriteStringValue(s);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}

internal class ByteSerializer : BaseSerializer<byte>
{
    public override byte Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int => reader.GetByte(),
            _ => throw new SerializationException(UnexpectedTokenExceptionMessage(reader.CurrentTokenType))
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case byte i:
                writer.WriteIntValue(i);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}

internal class SByteSerializer : BaseSerializer<sbyte>
{
    public override sbyte Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int => reader.GetUnsignedByte(),
            _ => throw new SerializationException(UnexpectedTokenExceptionMessage(reader.CurrentTokenType))
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case sbyte i:
                writer.WriteIntValue(i);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}


internal class ShortSerializer : BaseSerializer<short>
{
    public override short Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int => reader.GetShort(),
            _ => throw new SerializationException(UnexpectedTokenExceptionMessage(reader.CurrentTokenType))
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case short i:
                writer.WriteIntValue(i);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}

internal class UShortSerializer : BaseSerializer<ushort>
{
    public override ushort Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int => reader.GetUnsignedShort(),
            _ => throw new SerializationException(UnexpectedTokenExceptionMessage(reader.CurrentTokenType))
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case ushort i:
                writer.WriteIntValue(i);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}

internal class IntSerializer : BaseSerializer<int>
{
    public override int Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int => reader.GetInt(),
            _ => throw new SerializationException(UnexpectedTokenExceptionMessage(reader.CurrentTokenType))
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case int i:
                writer.WriteIntValue(i);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}

internal class UIntSerializer : BaseSerializer<uint>
{
    public override uint Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetUnsignedInt(),
            _ => throw new SerializationException(UnexpectedTokenExceptionMessage(reader.CurrentTokenType))
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case uint i:
                writer.WriteLongValue(i);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}

internal class LongSerializer : BaseSerializer<long>
{
    public override long Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long => reader.GetLong(),
            _ => throw new SerializationException(UnexpectedTokenExceptionMessage(reader.CurrentTokenType))
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case long i:
                writer.WriteLongValue(i);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}

internal class FloatSerializer : BaseSerializer<float>
{
    public override float Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long or TokenType.Double => reader.GetFloat(),
            _ => throw new SerializationException(UnexpectedTokenExceptionMessage(reader.CurrentTokenType))
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case float i:
                writer.WriteDoubleValue(i);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}

internal class DoubleSerializer : BaseSerializer<double>
{
    public override double Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int or TokenType.Long or TokenType.Double => reader.GetDouble(),
            _ => throw new SerializationException(UnexpectedTokenExceptionMessage(reader.CurrentTokenType))
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case double i:
                writer.WriteDoubleValue(i);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}

internal class BooleanSerializer : BaseSerializer<bool>
{
    public override bool Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.True or TokenType.False => reader.GetBoolean(),
            _ => throw new SerializationException(UnexpectedTokenExceptionMessage(reader.CurrentTokenType))
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case bool i:
                writer.WriteBooleanValue(i);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}

internal class DateOnlySerializer : BaseSerializer<DateOnly>
{
    public override DateOnly Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Date => reader.GetDate(),
            _ => throw new SerializationException(UnexpectedTokenExceptionMessage(reader.CurrentTokenType))
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case DateOnly i:
                writer.WriteDateValue(i);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}

internal class DateTimeSerializer : BaseSerializer<DateTime>
{
    public override DateTime Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Time => reader.GetTime(),
            _ => throw new SerializationException(UnexpectedTokenExceptionMessage(reader.CurrentTokenType))
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case DateTime i:
                writer.WriteTimeValue(i);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}

internal class DateTimeOffsetSerializer : BaseSerializer<DateTimeOffset>
{
    public override DateTimeOffset Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Time => reader.GetTime(),
            _ => throw new SerializationException(UnexpectedTokenExceptionMessage(reader.CurrentTokenType))
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case DateTimeOffset i:
                writer.WriteTimeValue(i);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}
