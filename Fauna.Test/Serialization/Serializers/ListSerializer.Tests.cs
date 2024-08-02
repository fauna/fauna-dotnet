using System.Collections;
using Fauna.Mapping;
using Fauna.Serialization;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

public class ListSerializerTests
{
    private static readonly MappingContext s_ctx = new();

    [Test]
    public void RoundTripListOfObjects()
    {
        var serializer = SerializerProvider.Generate<List<object>>(s_ctx);

        const string wire = @"[{""@int"":""42""},""foo bar"",[],{}]";
        var list = new List<object>
        {
            42,
            "foo bar",
            new List<object>(),
            new Dictionary<string, object>()
        };

        var deserialized = Helpers.Deserialize(serializer, s_ctx, wire);
        Assert.AreEqual(list, deserialized);

        string serialized = Helpers.Serialize(serializer, s_ctx, deserialized);
        Assert.AreEqual(wire, serialized);
    }

    [Test]
    public void RoundTripListOfIntegers()
    {
        var serializer = SerializerProvider.Generate<List<int>>(s_ctx);

        const string wire = @"[{""@int"":""42""},{""@int"":""42""},{""@int"":""42""},{""@int"":""42""}]";
        var list = new List<int> { 42, 42, 42, 42 };

        var deserialized = Helpers.Deserialize(serializer, s_ctx, wire);
        Assert.AreEqual(list, deserialized);

        string serialized = Helpers.Serialize(serializer, s_ctx, deserialized);
        Assert.AreEqual(wire, serialized);
    }

    [Test]
    public void RoundTripListOfClass()
    {
        var serializer = SerializerProvider.Generate<List<PersonWithAttributes>>(s_ctx);

        string wire = @"[
            {""first_name"":""Alice"",""last_name"":""Smith"",""age"":{""@int"":""100""}},
            {""first_name"":""Bob"",""last_name"":""Jones"",""age"":{""@int"":""101""}}
        ]".Replace(" ", "").Replace("\n", "");

        List<PersonWithAttributes> list = new()
        {
            new PersonWithAttributes { FirstName = "Alice", LastName = "Smith", Age = 100},
            new PersonWithAttributes { FirstName = "Bob", LastName = "Jones", Age = 101}
        };

        var deserialized = Helpers.Deserialize(serializer, s_ctx, wire);
        Assert.AreEqual(list, deserialized);

        string serialized = Helpers.Serialize(serializer, s_ctx, deserialized);
        Assert.AreEqual(wire, serialized);
    }

    [Test]
    public void RoundTripIEnumerable()
    {
        var serializer = SerializerProvider.Generate<IEnumerable<int>>(s_ctx);

        const string wire = @"[{""@int"":""42""},{""@int"":""42""},{""@int"":""42""},{""@int"":""42""}]";
        IEnumerable<int> list = new List<int> { 42, 42, 42, 42 };

        var deserialized = Helpers.Deserialize(serializer, s_ctx, wire);
        Assert.AreEqual(list, deserialized);

        string serialized = Helpers.Serialize(serializer, s_ctx, deserialized);
        Assert.AreEqual(wire, serialized);
    }

    [Test]
    public void DeserializeSingleValueAsList()
    {
        var serializer = SerializerProvider.Generate<List<string>>(s_ctx);

        const string wire = @"""foo""";
        var list = new List<string> { "foo" };

        var deserialized = Helpers.Deserialize(serializer, s_ctx, wire);
        Assert.AreEqual(list, deserialized);
    }
}
