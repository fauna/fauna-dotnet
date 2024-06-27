using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Types;
using NUnit.Framework;
using System.Text;
using System.Text.RegularExpressions;

namespace Fauna.Test.Serialization;

[TestFixture]
public class SerializerTests
{
    private static readonly MappingContext ctx = new();

    public static string Serialize(object? obj)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8FaunaWriter(stream);
        Serializer.Serialize(ctx, writer, obj);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    [Test]
    public void SerializeValues()
    {
        var dt = new DateTime(2023, 12, 13, 12, 12, 12, 1, DateTimeKind.Utc).AddTicks(10);

        var tests = new Dictionary<string, object?>
        {
            {"\"hello\"", "hello"},
            {"true", true},
            {"false", false},
            {"null", null},
            {@"{""@date"":""2023-12-13""}", new DateOnly(2023,12,13)},
            {@"{""@double"":""1.2""}", 1.2d},
            {@"{""@double"":""1.340000033378601""}", 1.34f},
            {@"{""@int"":""1""}", Convert.ToByte(1)},
            {@"{""@int"":""2""}", Convert.ToSByte(2)},
            {@"{""@int"":""40""}", short.Parse("40")},
            {@"{""@int"":""41""}", ushort.Parse("41")},
            {@"{""@int"":""42""}", 42},
            {@"{""@long"":""41""}", 41u},
            {@"{""@long"":""42""}", 42L},
            {@"{""@mod"":""module""}", new Module("module")},
            {@"{""@time"":""2023-12-13T12:12:12.0010010Z""}", dt},
            // \u002 is the + character. This is expected because we do not
            // enable unsafe json serialization. Fauna wire protocol supports this.
            {@"{""@time"":""2023-12-14T12:12:12.0010010\u002B00:00""}", new DateTimeOffset(dt.AddDays(1))},
        };

        foreach (var (expected, test) in tests)
        {
            var result = Serialize(test);
            Assert.AreEqual(expected, result);
        }
    }

    [Test]
    public void SerializeDictionary()
    {
        var test = new Dictionary<string, object>()
        {
            { "answer", 42 },
            { "foo", "bar" },
            { "list", new List<object>()},
            { "obj", new Dictionary<string, object>()}

        };

        var actual = Serialize(test);
        Assert.AreEqual(@"{""answer"":{""@int"":""42""},""foo"":""bar"",""list"":[],""obj"":{}}", actual);
    }

    [Test]
    public void SerializeDictionaryWithTagConflicts()
    {
        var tests = new Dictionary<Dictionary<string, object>, string>()
        {
            { new() { { "@date", "not" } }, @"{""@object"":{""@date"":""not""}}" },
            { new() { { "@doc", "not" } }, @"{""@object"":{""@doc"":""not""}}" },
            { new() { { "@double", "not" } }, @"{""@object"":{""@double"":""not""}}" },
            { new() { { "@int", "not" } }, @"{""@object"":{""@int"":""not""}}" },
            { new() { { "@long", "not" } }, @"{""@object"":{""@long"":""not""}}" },
            { new() { { "@mod", "not" } }, @"{""@object"":{""@mod"":""not""}}" },
            { new() { { "@object", "not" } }, @"{""@object"":{""@object"":""not""}}" },
            { new() { { "@ref", "not" } }, @"{""@object"":{""@ref"":""not""}}" },
            { new() { { "@set", "not" } }, @"{""@object"":{""@set"":""not""}}" },
            { new() { { "@time", "not" } }, @"{""@object"":{""@time"":""not""}}" }
        };

        foreach (var (test, expected) in tests)
        {
            var actual = Serialize(test);
            Assert.AreEqual(expected, actual);
        }
    }

    [Test]
    public void SerializeList()
    {
        var test = new List<object>()
        {
            42,
            "foo bar",
            new List<object>(),
            new Dictionary<string, object>()
        };

        var actual = Serialize(test);
        Assert.AreEqual(@"[{""@int"":""42""},""foo bar"",[],{}]", actual);
    }

    [Test]
    public void SerializeArray()
    {
        var test = new object[]
        {
            42,
            "foo bar",
            new object[] {},
            new Dictionary<string, object>()
        };

        var actual = Serialize(test);
        Assert.AreEqual(@"[{""@int"":""42""},""foo bar"",[],{}]", actual);
    }

    [Test]
    public void SerializeClass()
    {
        var test = new Person();
        var actual = Serialize(test);
        Assert.AreEqual(@"{""firstName"":""Baz"",""lastName"":""Luhrmann"",""age"":{""@int"":""61""}}", actual);
    }

    [Test]
    public void SerializeClassWithAttributes()
    {
        var test = new PersonWithAttributes();
        var actual = Serialize(test);
        Assert.AreEqual(@"{""first_name"":""Baz"",""last_name"":""Luhrmann"",""age"":{""@long"":""61""}}", actual);
    }

    [Test]
    public void SerializeClassWithTagConflicts()
    {
        var tests = new Dictionary<object, string>()
        {
            { new PersonWithDateConflict(), @"{""@object"":{""@date"":""not""}}" },
            { new PersonWithDocConflict(), @"{""@object"":{""@doc"":""not""}}" },
            { new PersonWithDoubleConflict(), @"{""@object"":{""@double"":""not""}}" },
            { new PersonWithIntConflict(), @"{""@object"":{""@int"":""not""}}" },
            { new PersonWithLongConflict(), @"{""@object"":{""@long"":""not""}}" },
            { new PersonWithModConflict(), @"{""@object"":{""@mod"":""not""}}" },
            { new PersonWithObjectConflict(), @"{""@object"":{""@object"":""not""}}" },
            { new PersonWithRefConflict(), @"{""@object"":{""@ref"":""not""}}" },
            { new PersonWithSetConflict(), @"{""@object"":{""@set"":""not""}}" },
            { new PersonWithTimeConflict(), @"{""@object"":{""@time"":""not""}}" }
        };

        foreach (var (test, expected) in tests)
        {
            var actual = Serialize(test);
            Assert.AreEqual(expected, actual);
        }
    }

    [Test]
    public void SerializeClassWithTypeConversions()
    {
        var test = new PersonWithTypeOverrides();
        var expectedWithWhitespace = @"
                       {
                           ""short_to_long"": {""@long"": ""10""},
                           ""ushort_to_long"": {""@long"": ""11""},
                           ""byte_to_long"": {""@long"": ""12""},
                           ""sbyte_to_long"": {""@long"": ""13""},
                           ""int_to_long"": {""@long"": ""20""},
                           ""uint_to_long"": {""@long"": ""21""},
                           ""long_to_long"": {""@long"": ""30""},
                           ""short_to_int"": {""@int"": ""40""},
                           ""ushort_to_int"": {""@int"": ""41""},
                           ""byte_to_int"": {""@int"": ""42""},
                           ""sbyte_to_int"": {""@int"": ""43""},
                           ""int_to_int"": {""@int"": ""50""},
                           ""short_to_double"": {""@double"": ""60""},
                           ""int_to_double"": {""@double"": ""70""},
                           ""long_to_double"": {""@double"": ""80""},
                           ""double_to_double"": {""@double"": ""10.1""},
                           ""float_to_double"": {""@double"": ""1.344499945640564""},
                           ""true_to_true"": true,
                           ""false_to_false"": false,
                           ""class_to_string"": ""TheThing"",
                           ""string_to_string"": ""aString"",
                           ""datetime_to_date"": {""@date"": ""2023-12-13""},
                           ""dateonly_to_date"": {""@date"": ""2023-12-13""},
                           ""datetimeoffset_to_date"": {""@date"": ""2023-12-13""},
                           ""datetime_to_time"": {""@time"":""2023-12-13T12:12:12.0010010Z""},
                           ""datetimeoffset_to_time"": {""@time"":""2023-12-13T12:12:12.0010010\u002B00:00""}
                       }
                       ";
        var expected = Regex.Replace(expectedWithWhitespace, @"\s", string.Empty);
        var actual = Serialize(test);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void SerializeObjectWithInvalidTypeHint()
    {
        var obj = new ClassWithInvalidPropertyTypeHint();
        Assert.Throws<SerializationException>(() => Serialize(obj));
    }

    [Test]
    public void SerializeObjectWithFieldAttributeAndWithoutObjectAttribute()
    {
        var obj = new ClassWithFieldAttributeAndWithoutObjectAttribute();
        var expected = "{\"firstName\":\"Baz\"}";
        var actual = Serialize(obj);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void SerializeObjectWithPropertyWithoutFieldAttribute()
    {
        var obj = new ClassWithPropertyWithoutFieldAttribute();
        var expected = "{}";
        var actual = Serialize(obj);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void SerializeAnonymousClassObject()
    {
        var obj = new { FirstName = "John", LastName = "Doe" };
        var expected = "{\"firstName\":\"John\",\"lastName\":\"Doe\"}";
        var actual = Serialize(obj);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void SerializeCollectionEdgeCases()
    {
        var tests = new Dictionary<object, string>
        {
            { new List<object>(), "[]" },
            { new Dictionary<string, object>(), "{}" },
            { new List<object> { new List<object>(), new Dictionary<string, object>() }, "[[],{}]" }
        };

        foreach (var (value, expected) in tests)
        {
            var actual = Serialize(value);
            Assert.AreEqual(expected, actual);
        }
    }

    [Test]
    public void Serialize_ExtremeNumericValues()
    {
        var tests = new Dictionary<object, string>
        {
            { short.MaxValue, @"{""@int"":""32767""}" },
            { short.MinValue, @"{""@int"":""-32768""}" },
            { int.MaxValue, @"{""@int"":""2147483647""}" },
            { int.MinValue, @"{""@int"":""-2147483648""}" },
            { long.MaxValue, @"{""@long"":""9223372036854775807""}" },
            { long.MinValue, @"{""@long"":""-9223372036854775808""}" },
            { double.MaxValue, @"{""@double"":""1.7976931348623157E\u002B308""}" },
            { double.MinValue, @"{""@double"":""-1.7976931348623157E\u002B308""}" }
        };

        foreach (var (value, expected) in tests)
        {
            var actual = Serialize(value);
            Assert.AreEqual(expected, actual);
        }
    }

    [Test]
    public void SerializeNumericEdgeCases()
    {
        var tests = new Dictionary<object, string>
        {
            { double.NaN, @"{""@double"":""NaN""}" },
            { double.PositiveInfinity, @"{""@double"":""Infinity""}" },
            { double.NegativeInfinity, @"{""@double"":""-Infinity""}" }
        };

        foreach (var (value, expected) in tests)
        {
            var actual = Serialize(value);
            Assert.AreEqual(expected, actual);
        }
    }

    [Test]
    public void SerializeNullableStructAsNull()
    {
        var i = new int?();

        var actual = Serialize(i);
        Assert.AreEqual("null", actual);
    }

    [Test]
    public void SerializeNullableStructAsValue()
    {
        var i = new int?(42);

        var actual = Serialize(i);
        Assert.AreEqual(@"{""@int"":""42""}", actual);
    }
}
