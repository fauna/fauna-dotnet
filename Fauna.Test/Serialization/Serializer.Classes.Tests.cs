using Fauna.Serialization;
using Fauna.Serialization.Attributes;

namespace Fauna.Test.Serialization;

public partial class SerializerTests
{
    private class Person
    {
        public string? FirstName { get; set; } = "Baz";
        public string? LastName { get; set; } = "Luhrmann";
        public int Age { get; set; } = 61;
    }

    [FaunaObject]
    private class PersonWithAttributes
    {
        [Field("first_name")] public string? FirstName { get; set; } = "Baz";
        [Field("last_name")] public string? LastName { get; set; } = "Luhrmann";
        [Field("age", FaunaType.Long)] public int Age { get; set; } = 61;
        public string? Ignored { get; set; }
    }

    [FaunaObject]
    private class ClassWithInvalidPropertyTypeHint
    {
        [Field("first_name", FaunaType.Int)] public string FirstName { get; set; } = "NotANumber";
    }

    private class ClassWithoutFaunaObjectAttribute
    {
        [Field("first_name")] public string? FirstName { get; set; } = "Baz";
    }

    [FaunaObject]
    private class ClassWithPropertyWithoutFieldAttribute
    {
        public string FirstName { get; set; } = "NotANumber";
    }

    private class ClassWithoutFieldAttribute
    {
        public string FirstName { get; set; } = "NotANumber";
    }

    private class ThingWithStringOverride
    {
        private const string Name = "TheThing";

        public override string ToString()
        {
            return Name;
        }
    }

    [FaunaObject]
    private class PersonWithTypeOverrides
    {
        // Long Conversions
        [Field("short_to_long", FaunaType.Long)] public short? ShortToLong { get; set; } = 1;
        [Field("int_to_long", FaunaType.Long)] public int? IntToLong { get; set; } = 2;
        [Field("long_to_long", FaunaType.Long)] public long? LongToLong { get; set; } = 3L;

        // Int Conversions
        [Field("short_to_int", FaunaType.Int)] public short? ShortToInt { get; set; } = 4;
        [Field("int_to_int", FaunaType.Int)] public int? IntToInt { get; set; } = 5;

        // Double Conversions
        [Field("short_to_double", FaunaType.Double)] public short? ShortToDouble { get; set; } = 6;
        [Field("int_to_double", FaunaType.Double)] public int? IntToDouble { get; set; } = 7;
        [Field("long_to_double", FaunaType.Double)] public long? LongToDouble { get; set; } = 8L;
        [Field("decimal_to_double", FaunaType.Double)] public decimal? DecimalToDouble { get; set; } = 9.2M;
        [Field("double_to_double", FaunaType.Double)] public double? DoubleToDouble { get; set; } = 10.1d;

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


    [FaunaObject]
    private class PersonWithIntConflict
    {
        [Field("@int")] public string? Field { get; set; } = "not";
    }

    [FaunaObject]
    private class PersonWithLongConflict
    {
        [Field("@long")] public string? Field { get; set; } = "not";
    }

    [FaunaObject]
    private class PersonWithDoubleConflict
    {
        [Field("@double")] public string? Field { get; set; } = "not";
    }

    [FaunaObject]
    private class PersonWithModConflict
    {
        [Field("@mod")] public string? Field { get; set; } = "not";
    }

    [FaunaObject]
    private class PersonWithRefConflict
    {
        [Field("@ref")] public string? Field { get; set; } = "not";
    }

    [FaunaObject]
    private class PersonWithDocConflict
    {
        [Field("@doc")] public string? Field { get; set; } = "not";
    }

    [FaunaObject]
    private class PersonWithObjectConflict
    {
        [Field("@object")] public string? Field { get; set; } = "not";
    }

    [FaunaObject]
    private class PersonWithSetConflict
    {
        [Field("@set")] public string? Field { get; set; } = "not";
    }

    [FaunaObject]
    private class PersonWithTimeConflict
    {
        [Field("@time")] public string? Field { get; set; } = "not";
    }

    [FaunaObject]
    private class PersonWithDateConflict
    {
        [Field("@date")] public string? Field { get; set; } = "not";
    }

}