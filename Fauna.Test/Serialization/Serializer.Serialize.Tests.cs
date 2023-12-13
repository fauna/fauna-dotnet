using System.Text.RegularExpressions;
using Fauna.Serialization;
using Fauna.Serialization.Attributes;
using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

[TestFixture]
public partial class SerializerTests
{
    [Test]
    public void SerializeValues()
    {
        var dt = new DateTime(2023, 12, 13, 12, 12, 12, 1, 1, DateTimeKind.Utc);
        
        var tests = new Dictionary<string, object?>
        {
            {"\"hello\"", "hello"},
            {"true", true},
            {"false", false},
            {"null", null},
            {"""{"@date":"2023-12-13"}""", new DateOnly(2023,12,13)},
            {"""{"@double":"1.2"}""", 1.2d},
            {"""{"@double":"3.14"}""", 3.14M},
            {"""{"@int":"42"}""", 42},
            {"""{"@long":"42"}""", 42L},
            {"""{"@mod":"module"}""", new Module("module")},
            {"""{"@time":"2023-12-13T12:12:12.0010010Z"}""", dt},
            // \u002 is the + character. This is expected because we do not
            // enable unsafe json serialization. Fauna wire protocol supports this.
            {"""{"@time":"2023-12-14T12:12:12.0010010\u002B00:00"}""", new DateTimeOffset(dt.AddDays(1))},
        };

        foreach (var (expected, test) in tests)
        {
            var result = Serializer.Serialize(test);
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

        var actual = Serializer.Serialize(test);
        Assert.AreEqual("""{"answer":{"@int":"42"},"foo":"bar","list":[],"obj":{}}""", actual);
    }

    [Test]
    public void SerializeDictionaryWithTagConflicts()
    {
        var tests = new Dictionary<Dictionary<string, object>, string>()
        {
            { new() { { "@date", "not" } }, """{"@object":{"@date":"not"}}""" },
            { new() { { "@doc", "not" } }, """{"@object":{"@doc":"not"}}""" },
            { new() { { "@double", "not" } }, """{"@object":{"@double":"not"}}""" },
            { new() { { "@int", "not" } }, """{"@object":{"@int":"not"}}""" },
            { new() { { "@long", "not" } }, """{"@object":{"@long":"not"}}""" },
            { new() { { "@mod", "not" } }, """{"@object":{"@mod":"not"}}""" },
            { new() { { "@object", "not" } }, """{"@object":{"@object":"not"}}""" },
            { new() { { "@ref", "not" } }, """{"@object":{"@ref":"not"}}""" },
            { new() { { "@set", "not" } }, """{"@object":{"@set":"not"}}""" },
            { new() { { "@time", "not" } }, """{"@object":{"@time":"not"}}""" }
        };

        foreach (var (test, expected) in tests)
        {
            var actual = Serializer.Serialize(test);
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

        var actual = Serializer.Serialize(test);
        Assert.AreEqual("""[{"@int":"42"},"foo bar",[],{}]""", actual);
    }
    
    [Test]
    public void SerializeClass()
    {
        var test = new Person();
        var actual = Serializer.Serialize(test);
        Assert.AreEqual("""{"FirstName":"Baz","LastName":"Luhrmann","Age":{"@int":"61"}}""", actual);
    }

    [Test]
    public void SerializeClassWithAttributes()
    {
        var test = new PersonWithAttributes();
        var actual = Serializer.Serialize(test);
        Assert.AreEqual("""{"first_name":"Baz","last_name":"Luhrmann","age":{"@long":"61"}}""", actual);
    }

    [Test]
    public void SerializeClassWithTagConflicts()
    {
        var tests = new Dictionary<object, string>()
        {
            { new PersonWithDateConflict(), """{"@object":{"@date":"not"}}""" },
            { new PersonWithDocConflict(), """{"@object":{"@doc":"not"}}""" },
            { new PersonWithDoubleConflict(), """{"@object":{"@double":"not"}}""" },
            { new PersonWithIntConflict(), """{"@object":{"@int":"not"}}""" },
            { new PersonWithLongConflict(), """{"@object":{"@long":"not"}}""" },
            { new PersonWithModConflict(), """{"@object":{"@mod":"not"}}""" },
            { new PersonWithObjectConflict(), """{"@object":{"@object":"not"}}""" },
            { new PersonWithRefConflict(), """{"@object":{"@ref":"not"}}""" },
            { new PersonWithSetConflict(), """{"@object":{"@set":"not"}}""" },
            { new PersonWithTimeConflict(), """{"@object":{"@time":"not"}}""" }
        };
        
        foreach (var (test, expected) in tests)
        {
            var actual = Serializer.Serialize(test);
            Assert.AreEqual(expected, actual);
        }
    }

    [Test]
    public void SerializeClassWithTypeConversions()
    {
        var test = new PersonWithTypeOverrides();
        var expectedWithWhitespace = """
                       {
                           "short_to_long": {"@long": "1"},
                           "int_to_long": {"@long": "2"},
                           "long_to_long": {"@long": "3"},
                           "short_to_int": {"@int": "4"},
                           "int_to_int": {"@int": "5"},
                           "short_to_double": {"@double": "6"},
                           "int_to_double": {"@double": "7"},
                           "long_to_double": {"@double": "8"},
                           "decimal_to_double": {"@double": "9.2"},
                           "double_to_double": {"@double": "10.1"},
                           "true_to_true": true,
                           "false_to_false": false,
                           "class_to_string": "TheThing",
                           "string_to_string": "aString",
                           "datetime_to_date": {"@date": "2023-12-13"},
                           "dateonly_to_date": {"@date": "2023-12-13"},
                           "datetimeoffset_to_date": {"@date": "2023-12-13"},
                           "datetime_to_time": {"@time":"2023-12-13T12:12:12.0010010Z"},
                           "datetimeoffset_to_time": {"@time":"2023-12-13T12:12:12.0010010\u002B00:00"}
                       }
                       """;
        var expected = Regex.Replace(expectedWithWhitespace, @"\s", string.Empty);
        var actual = Serializer.Serialize(test);
        Assert.AreEqual(expected, actual);
    }
}
