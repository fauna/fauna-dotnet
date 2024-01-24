using Fauna.Mapping.Attributes;

namespace Fauna.Test.Serialization;

class Person
{
    public string? FirstName { get; set; } = "Baz";
    public string? LastName { get; set; } = "Luhrmann";
    public int Age { get; set; } = 61;
}

[Object]
class ClassForDocument
{
    [Field] public long Id { get; set; }
    [Field("user_field")] public string? UserField { get; set; }
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
    [Field("age", FaunaType.Long)] public int Age { get; set; } = 61;
    public string? Ignored { get; set; }
}


[Object]
class ClassWithInvalidPropertyTypeHint
{
    [Field("first_name", FaunaType.Int)] public string FirstName { get; set; } = "NotANumber";
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
class PersonWithTypeOverrides
{
    // Long Conversions
    [Field("short_to_long", FaunaType.Long)] public short? ShortToLong { get; set; } = 10;
    [Field("ushort_to_long", FaunaType.Long)] public ushort? UShortToLong { get; set; } = 11;
    [Field("byte_to_long", FaunaType.Long)] public byte? ByteToLong { get; set; } = 12;
    [Field("sbyte_to_long", FaunaType.Long)] public sbyte? SByteToLong { get; set; } = 13;
    [Field("int_to_long", FaunaType.Long)] public int? IntToLong { get; set; } = 20;
    [Field("uint_to_long", FaunaType.Long)] public uint? UIntToLong { get; set; } = 21;
    [Field("long_to_long", FaunaType.Long)] public long? LongToLong { get; set; } = 30L;

    // Int Conversions
    [Field("short_to_int", FaunaType.Int)] public short? ShortToInt { get; set; } = 40;
    [Field("ushort_to_int", FaunaType.Int)] public short? UShortToInt { get; set; } = 41;
    [Field("byte_to_int", FaunaType.Int)] public byte? ByteToInt { get; set; } = 42;
    [Field("sbyte_to_int", FaunaType.Int)] public sbyte? SByteToInt { get; set; } = 43;
    [Field("int_to_int", FaunaType.Int)] public int? IntToInt { get; set; } = 50;

    // Double Conversions
    [Field("short_to_double", FaunaType.Double)] public short? ShortToDouble { get; set; } = 60;
    [Field("int_to_double", FaunaType.Double)] public int? IntToDouble { get; set; } = 70;
    [Field("long_to_double", FaunaType.Double)] public long? LongToDouble { get; set; } = 80L;
    [Field("double_to_double", FaunaType.Double)] public double? DoubleToDouble { get; set; } = 10.1d;
    [Field("float_to_double", FaunaType.Double)] public float? FloatToDouble { get; set; } = 1.3445f;

    // Bool conversions
    [Field("true_to_true", FaunaType.Boolean)] public bool? TrueToTrue { get; set; } = true;
    [Field("false_to_false", FaunaType.Boolean)] public bool? FalseToFalse { get; set; } = false;

    // String conversions
    [Field("class_to_string", FaunaType.String)]
    public ThingWithStringOverride? ThingToString { get; set; } = new();
    [Field("string_to_string", FaunaType.String)] public string? StringToString { get; set; } = "aString";

    // Date conversions
    [Field("datetime_to_date", FaunaType.Date)]
    public DateTime? DateTimeToDate { get; set; } = DateTime.Parse("2023-12-13T12:12:12.001001Z");
    [Field("dateonly_to_date", FaunaType.Date)]
    public DateOnly? DateOnlyToDate { get; set; } = new DateOnly(2023, 12, 13);
    [Field("datetimeoffset_to_date", FaunaType.Date)]
    public DateTimeOffset? DateTimeOffsetToDate { get; set; } = DateTimeOffset.Parse("2023-12-13T12:12:12.001001Z");

    // Time conversions
    [Field("datetime_to_time", FaunaType.Time)]
    public DateTime? DateTimeToTime { get; set; } = DateTime.Parse("2023-12-13T12:12:12.001001Z");
    [Field("datetimeoffset_to_time", FaunaType.Time)]
    public DateTimeOffset? DateTimeOffsetToTime { get; set; } = new DateTimeOffset(DateTime.Parse("2023-12-13T12:12:12.001001Z"));
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
