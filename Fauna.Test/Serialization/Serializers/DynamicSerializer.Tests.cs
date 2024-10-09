
using System.Reflection.Metadata;
using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

public class DynamicSerializerTests
{
    private readonly MappingContext _ctx;
    private const string DocumentWire = @"{""@doc"":{""id"":""123"",""coll"":{""@mod"":""MyColl""},""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},""user_field"":""user_value""}}";
    private const string RefWire = @"{""@ref"":{""id"":""123"",""coll"":{""@mod"":""MyColl""}}}";
    private const string NullDocumentWire = @"{""@ref"":{""id"":""123"",""coll"":{""@mod"":""MyColl""},""exists"":false,""cause"":""not found""}}";
    private const string NamedDocumentWire = @"{""@doc"":{""name"":""Foo"",""coll"":{""@mod"":""MyColl""},""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},""user_field"":""user_value""}}";
    private const string NamedRefWire = @"{""@ref"":{""name"":""Foo"",""coll"":{""@mod"":""MyColl""}}}";
    private const string NullNamedDocumentWire = @"{""@ref"":{""name"":""Foo"",""coll"":{""@mod"":""MyColl""},""exists"":false,""cause"":""not found""}}";

    public DynamicSerializerTests()
    {
        var collections = new Dictionary<string, Type> {
            { "MappedColl", typeof(ClassForDocument) }
        };
        _ctx = new MappingContext(collections);
    }

    [Test]
    public void RoundTripValues()
    {
        var tests = new Dictionary<string, object?>
        {
            {"\"hello\"", "hello"},
            {@"{""@int"":""42""}", 42},
            {@"{""@long"":""42""}", 42L},
            {@"{""@double"":""1.2""}", 1.2d},
            {@"{""@date"":""2023-12-03""}", new DateOnly(2023, 12, 3)},
            {@"{""@time"":""2023-12-03T05:52:10.000001-09:00""}", new DateTime(2023, 12, 3, 14, 52, 10, 0, DateTimeKind.Utc).AddTicks(10).ToLocalTime()},
            {"true", true},
            {"false", false},
            {"null", null},
        };

        foreach (KeyValuePair<string, object?> entry in tests)
        {
            object? deserialized = Helpers.Deserialize(Serializer.Dynamic, _ctx, entry.Key);
            Assert.AreEqual(entry.Value, deserialized);
            string serialized = Helpers.Serialize(Serializer.Dynamic, _ctx, deserialized);
            Assert.AreEqual(entry.Value is DateTime ? "{\"@time\":\"2023-12-03T14:52:10.0000010Z\"}" : entry.Key,
                serialized);
        }
    }

    [Test]
    public void RoundTripNestedEmptyCollections()
    {
        const string wire = "[[],{}]";
        var expected = new List<object> { new List<object>(), new Dictionary<string, object>() };
        object? deserialized = Helpers.Deserialize(Serializer.Dynamic, _ctx, wire);
        Assert.AreEqual(expected, deserialized);
        string serialized = Helpers.Serialize(Serializer.Dynamic, _ctx, deserialized);
        Assert.AreEqual(wire, serialized);
    }

    [Test]
    public void RoundTripDocument()
    {
        object? deserialized = Helpers.Deserialize(Serializer.Dynamic, _ctx, DocumentWire);
        switch (deserialized)
        {
            case Ref<Dictionary<string, object>> d:
                Assert.AreEqual("123", d.Id);
                Assert.AreEqual(new Module("MyColl"), d.Collection);
                var dict = d.Get();
                Assert.AreEqual("123", dict["id"]);
                Assert.AreEqual(new Module("MyColl"), dict["coll"]);
                Assert.AreEqual(DateTime.Parse("2023-12-15T01:01:01.0010010Z"), dict["ts"]);
                Assert.AreEqual("user_value", dict["user_field"]);
                break;
            default:
                Assert.Fail($"result is type: {deserialized?.GetType()}");
                break;
        }

        string serialized = Helpers.Serialize(Serializer.Dynamic, _ctx, deserialized);
        Assert.AreEqual(RefWire, serialized);
    }

    [Test]
    public void RoundTripNullDocument()
    {
        object? deserialized = Helpers.Deserialize(Serializer.Dynamic, _ctx, NullDocumentWire);
        switch (deserialized)
        {
            case Ref<Dictionary<string, object>> d:
                Assert.AreEqual("123", d.Id);
                Assert.AreEqual("MyColl", d.Collection.Name);
                Assert.AreEqual("not found", d.Cause);
                Assert.IsFalse(d.Exists);
                break;
            default:
                Assert.Fail($"result is type: {deserialized?.GetType()}");
                break;
        }

        string serialized = Helpers.Serialize(Serializer.Dynamic, _ctx, deserialized);
        Assert.AreEqual(RefWire, serialized);
    }

    [Test]
    public void RoundTripNamedDocument()
    {
        object? deserialized = Helpers.Deserialize(Serializer.Dynamic, _ctx, NamedDocumentWire);
        switch (deserialized)
        {
            case NamedRef<Dictionary<string, object>> d:
                Assert.AreEqual("Foo", d.Name);
                Assert.AreEqual("MyColl", d.Collection.Name);
                var doc = d.Get();

                Assert.AreEqual(DateTime.Parse("2023-12-15T01:01:01.0010010Z"), doc["ts"]);
                Assert.AreEqual("user_value", doc["user_field"]);
                break;
            default:
                Assert.Fail($"result is type: {deserialized?.GetType()}");
                break;
        }

        string serialized = Helpers.Serialize(Serializer.Dynamic, _ctx, deserialized);
        Assert.AreEqual(NamedRefWire, serialized);
    }

    [Test]
    public void RoundTripNullNamedDocument()
    {
        object? deserialized = Helpers.Deserialize(Serializer.Dynamic, _ctx, NullNamedDocumentWire);

        switch (deserialized)
        {
            case NamedRef<Dictionary<string, object>> d:
                Assert.AreEqual("Foo", d.Name);
                Assert.AreEqual("MyColl", d.Collection.Name);
                Assert.AreEqual("not found", d.Cause);
                Assert.IsFalse(d.Exists);
                break;
            default:
                Assert.Fail($"result is type: {deserialized?.GetType()}");
                break;
        }

        string serialized = Helpers.Serialize(Serializer.Dynamic, _ctx, deserialized);
        Assert.AreEqual(NamedRefWire, serialized);
    }

    [Test]
    public void RoundTripAnonymousClass()
    {
        var obj = new { FirstName = "John", LastName = "Doe" };
        const string wire = "{\"firstName\":\"John\",\"lastName\":\"Doe\"}";
        object? deserialized = Helpers.Deserialize(Serializer.Dynamic, _ctx, wire);
        string serialized = Helpers.Serialize(Serializer.Dynamic, _ctx, obj);
        Assert.AreEqual(wire, serialized);
    }

    [Test]
    public void RoundTripMappedClass()
    {
        const string mapped = @"{""@doc"":{""id"":""123"",""coll"":{""@mod"":""MappedColl""},""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},""user_field"":""user_value""}}";
        var obj = new ClassForDocument
        {
            Id = "123",
            Coll = new Module("MappedColl"),
            Ts = DateTime.Parse("2023-12-15T01:01:01.0010010Z"),
            UserField = "user_value"
        };

        object? deserialized = Helpers.Deserialize(Serializer.Dynamic, _ctx, mapped);
        switch (deserialized)
        {
            case Ref<object> c:
                Assert.AreEqual(obj.Id, c.Id);
                Assert.AreEqual(obj.Coll, c.Collection);

                var resObj = (ClassForDocument)c.Get();
                Assert.AreEqual(obj.Ts, resObj.Ts);
                Assert.AreEqual(obj.UserField, resObj.UserField);
                break;
            default:
                Assert.Fail($"result is type: {deserialized?.GetType()}");
                break;
        }

        string serialized = Helpers.Serialize(Serializer.Dynamic, _ctx, deserialized);
        Assert.AreEqual("{\"@ref\":{\"id\":\"123\",\"coll\":{\"@mod\":\"MappedColl\"}}}", serialized);
    }
}
