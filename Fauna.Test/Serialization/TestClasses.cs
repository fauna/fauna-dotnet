using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Types;

namespace Fauna.Test.Serialization;

class Person
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public int Age { get; init; }

    public override bool Equals(object? obj)
    {
        var item = obj as Person;

        if (item == null)
        {
            return false;
        }

        return FirstName == item.FirstName && LastName == item.LastName && Age == item.Age;
    }

    public override int GetHashCode()
    {
        return $"{FirstName ?? ""}{LastName ?? ""}{Age}".GetHashCode();
    }
}

class ClassForDocument
{
    [Id] public string? Id { get; set; }
    [Collection] public Module? Coll { get; set; }
    [Ts] public DateTime? Ts { get; set; }
    [Field("user_field")] public string? UserField { get; set; }
}

class ClassForDocumentClientGeneratedId
{
    [Id(true)] public string? Id { get; set; }
    [Collection] public Module? Coll { get; set; }
    [Ts] public DateTime? Ts { get; set; }
    [Field("user_field")] public string? UserField { get; set; }
}

class ClassForDocumentWithSpecialNames
{
    [Id] public string? TheId { get; set; }
    [Collection] public Module? TheCollection { get; set; }
    [Ts] public DateTime? TheTs { get; set; }
    [Field("user_field")] public string? UserField { get; set; }
}


class ClassForUnmapped
{
    [Id] public string? Id { get; set; }
    [Collection] public Module? Coll { get; set; }
    [Ts] public DateTime? Ts { get; set; }
    [Field("user_field")] public string? UserField { get; set; }
}

class ClassWithShort
{
    [Field("a_short")] public short AShort { get; set; }
}

class ClassForDocumentWithIdString
{
    [Id] public string? Id { get; set; }
    [Field("user_field")] public string? UserField { get; set; }
}

class ClassForDocumentWithInvalidId
{
    [Id] public bool Id { get; set; }
}

class ClassForNamedDocument
{
    [Field("name")] public string? Name { get; set; }
    [Field("user_field")] public string? UserField { get; set; }
}

class PersonWithAttributes
{
    [Field("first_name")] public string? FirstName { get; set; } = "Baz";
    [Field("last_name")] public string? LastName { get; set; } = "Luhrmann";
    [Field("age")] public int Age { get; set; } = 61;
    [Ignore] public string? Ignored { get; set; }

    public override bool Equals(object? obj)
    {
        var item = obj as PersonWithAttributes;

        if (item == null)
        {
            return false;
        }

        return FirstName == item.FirstName && LastName == item.LastName && Age == item.Age && Ignored == item.Ignored;
    }

    public override int GetHashCode()
    {
        return $"{FirstName ?? ""}{LastName ?? ""}{Age}{Ignored ?? ""}".GetHashCode();
    }
}

interface IOnlyField
{
    public string? Field { get; set; }
}


class PersonWithIntConflict : IOnlyField
{
    [Field("@int")] public string? Field { get; set; } = "not";
}


class PersonWithLongConflict : IOnlyField
{
    [Field("@long")] public string? Field { get; set; } = "not";
}


class PersonWithDoubleConflict : IOnlyField
{
    [Field("@double")] public string? Field { get; set; } = "not";
}


class PersonWithModConflict : IOnlyField
{
    [Field("@mod")] public string? Field { get; set; } = "not";
}


class PersonWithRefConflict : IOnlyField
{
    [Field("@ref")] public string? Field { get; set; } = "not";
}


class PersonWithDocConflict : IOnlyField
{
    [Field("@doc")] public string? Field { get; set; } = "not";
}


class PersonWithObjectConflict : IOnlyField
{
    [Field("@object")] public string? Field { get; set; } = "not";
}


class PersonWithSetConflict : IOnlyField
{
    [Field("@set")] public string? Field { get; set; } = "not";
}


class PersonWithTimeConflict : IOnlyField
{
    [Field("@time")] public string? Field { get; set; } = "not";
}


class PersonWithDateConflict : IOnlyField
{
    [Field("@date")] public string? Field { get; set; } = "not";
}


class ClassWithDupeFields
{
    [Id] public string? Id { get; set; }
    [Collection] public Module? Coll { get; set; }
    [Ts] public DateTime? Ts { get; set; }
    [Field("user_field")] public string? UserField { get; set; }
    [Field("user_field")] public string? UserField2 { get; set; }
}


class ClassWithFieldNameOverlap
{
    [Id] public string? Id { get; set; }
    [Collection] public Module? Coll { get; set; }
    [Ts] public DateTime? Ts { get; set; }
    [Field("user_field")] public string? UserField { get; set; }
    [Field] public string? user_field { get; set; }
}


class ClassWithLotsOfFields
{
    [Id] public string? Id { get; set; }
    [Collection] public Module? Coll { get; set; }
    [Ts] public DateTime? Ts { get; set; }
    [Field] public DateTime DateTimeField { get; set; }
    [Field] public DateOnly DateOnlyField { get; set; }
    [Field] public DateTimeOffset DateTimeOffsetField { get; set; }
    [Field] public string? StringField { get; set; }
    [Field] public short ShortField { get; set; }
    [Field] public ushort UshortField { get; set; }
    [Field] public int IntField { get; set; }
    [Field] public uint UintField { get; set; }
    [Field] public float FloatField { get; set; }
    [Field] public double DoubleField { get; set; }
    [Field] public long LongField { get; set; }
    [Field] public bool BoolField { get; set; }
    [Field] public byte ByteField { get; set; }
    [Field] public sbyte SbyteField { get; set; }
    [Field] public int? NullableIntField { get; set; }
    [Field] public ClassForDocument? OtherDocRef { get; set; }
}

public class IntToStringSerializer : BaseSerializer<int>
{
    public override int Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        return reader.CurrentTokenType switch
        {
            TokenType.String => int.Parse(reader.GetString() ?? "0"),
            _ => throw new SerializationException(UnexpectedTypeDecodingMessage(reader.CurrentTokenType.GetFaunaType()))
        };
    }

    public override void Serialize(MappingContext ctx, Utf8FaunaWriter w, object? o)
    {
        switch (o)
        {
            case null:
                w.WriteNullValue();
                break;
            case int v:
                w.WriteStringValue(v.ToString());
                break;
            case string v:
                w.WriteStringValue(v);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}
