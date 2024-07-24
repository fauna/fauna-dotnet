using Fauna.Mapping;
using Fauna.Mapping.Attributes;
using Fauna.Serialization;
using Fauna.Types;

namespace Fauna.Test.Serialization;

class Person
{
    public string? FirstName { get; set; } = "Baz";
    public string? LastName { get; set; } = "Luhrmann";
    public int Age { get; set; } = 61;
}

class NullableInt
{
    public int? Val { get; set; }
}

[Object]
class ClassForDocument
{
    [Field] public long Id { get; set; }
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
}

class ClassWithFieldAttributeAndWithoutObjectAttribute
{
    [Field("first_name")] public string? FirstName { get; set; } = "Baz";
}

[Object]
class ClassWithPropertyWithoutFieldAttribute
{
    public string FirstName { get; set; } = "NotANumber";
}

class ClassWithoutFieldAttribute
{
    public string FirstName { get; set; } = "NotANumber";
}

class ThingWithStringOverride
{
    private const string Name = "TheThing";

    public override string ToString()
    {
        return Name;
    }
}

[Object]
class PersonWithIntConflict
{
    [Field("@int")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithLongConflict
{
    [Field("@long")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithDoubleConflict
{
    [Field("@double")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithModConflict
{
    [Field("@mod")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithRefConflict
{
    [Field("@ref")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithDocConflict
{
    [Field("@doc")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithObjectConflict
{
    [Field("@object")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithSetConflict
{
    [Field("@set")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithTimeConflict
{
    [Field("@time")] public string? Field { get; set; } = "not";
}

[Object]
class PersonWithDateConflict
{
    [Field("@date")] public string? Field { get; set; } = "not";
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
