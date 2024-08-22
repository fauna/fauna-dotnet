using Fauna.Mapping;
using Fauna.Mapping.Attributes;
using Fauna.Serialization;
using Fauna.Types;

namespace Fauna.Test.Serialization;

class Person
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int Age { get; set; }

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

class NullableInt
{
    public int? Val { get; set; }
}

[Object]
class ClassForDocument
{
    [Field] public string? Id { get; set; }
    [Field] public Module? Coll { get; set; }
    [Field] public DateTime? Ts { get; set; }
    [Field("user_field")] public string? UserField { get; set; }
}

[Object]
class ClassForUnmapped
{
    [Field] public string? Id { get; set; }
    [Field] public Module? Coll { get; set; }
    [Field] public DateTime? Ts { get; set; }
    [Field("user_field")] public string? UserField { get; set; }
}

[Object]
class ClassWithShort
{
    [Field("a_short")] public short AShort { get; set; }
}

[Object]
class ClassForDocumentWithIdString
{
    [Field] public string? Id { get; set; }
    [Field("user_field")] public string? UserField { get; set; }
}

[Object]
class ClassForDocumentWithInvalidId
{
    [Field] public bool Id { get; set; }
}

[Object]
class ClassForNamedDocument
{
    [Field("name")] public string? Name { get; set; }
    [Field("user_field")] public string? UserField { get; set; }
}

[Object]
class PersonWithAttributes
{
    [Field("first_name")] public string? FirstName { get; set; } = "Baz";
    [Field("last_name")] public string? LastName { get; set; } = "Luhrmann";
    [Field("age")] public int Age { get; set; } = 61;
    public string? Ignored { get; set; }

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

[Object]
class PersonWithIntConflict : IOnlyField
{
    [Field("@int")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithLongConflict : IOnlyField
{
    [Field("@long")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithDoubleConflict : IOnlyField
{
    [Field("@double")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithModConflict : IOnlyField
{
    [Field("@mod")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithRefConflict : IOnlyField
{
    [Field("@ref")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithDocConflict : IOnlyField
{
    [Field("@doc")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithObjectConflict : IOnlyField
{
    [Field("@object")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithSetConflict : IOnlyField
{
    [Field("@set")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithTimeConflict : IOnlyField
{
    [Field("@time")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithDateConflict : IOnlyField
{
    [Field("@date")] public string? Field { get; set; } = "not";
}

[Object]
class ClassWithDupeFields
{
    [Field] public string? Id { get; set; }
    [Field] public Module? Coll { get; set; }
    [Field] public DateTime? Ts { get; set; }
    [Field("user_field")] public string? UserField { get; set; }
    [Field("user_field")] public string? UserField2 { get; set; }
}

[Object]
class ClassWithFieldNameOverlap
{
    [Field] public string? Id { get; set; }
    [Field] public Module? Coll { get; set; }
    [Field] public DateTime? Ts { get; set; }
    [Field("user_field")] public string? UserField { get; set; }
    [Field] public string? user_field { get; set; }
}

[Object]
class ClassWithLotsOfFields
{
    [Field] public string? Id { get; set; }
    [Field] public Module? Coll { get; set; }
    [Field] public DateTime? Ts { get; set; }
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
            _ => throw new SerializationException(UnexpectedTokenExceptionMessage(reader.CurrentTokenType))
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
