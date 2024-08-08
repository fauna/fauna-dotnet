using System.Text;
using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

[TestFixture]
public class SerializerTests
{
    private static readonly MappingContext ctx = new();

    public static string Serialize(object? obj)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8FaunaWriter(stream);

        ISerializer ser = DynamicSerializer.Singleton;
        if (obj is not null) ser = Serializer.Generate(ctx, obj.GetType());
        ser.Serialize(ctx, writer, obj);

        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    [Test]
    public void SerializeValues()
    {
        var dt = new DateTime(2023, 12, 13, 12, 12, 12, 1, DateTimeKind.Utc).AddTicks(10);

        var tests = new Dictionary<string, object?>
        {
            {"null", null},
            {@"{""@mod"":""module""}", new Module("module")},
            {@"{""@ref"":{""id"":""123"",""coll"":{""@mod"":""ACollection""}}}", new Ref("123", new Module("ACollection"))},
            {@"{""@ref"":{""id"":""124"",""coll"":{""@mod"":""ACollection""}}}", new Document("124", new Module("ACollection"), DateTime.Now)}
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
    public void SerializeAnonymousClassObject()
    {
        var obj = new { FirstName = "John", LastName = "Doe" };
        var expected = "{\"firstName\":\"John\",\"lastName\":\"Doe\"}";
        var actual = Serialize(obj);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void SerializeEmptyCollections()
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
