using System.Text.RegularExpressions;
using Fauna.Mapping;
using Fauna.Serialization;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

public class DictionarySerializerTests
{
    private static readonly MappingContext s_ctx = new();

    [Test]
    public void RoundTripDictionaryWithTagConflicts()
    {
        var serializer = Serializer.Generate<Dictionary<string, object>>(s_ctx);
        var tests = new Dictionary<Dictionary<string, object>, string>
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

        foreach ((Dictionary<string, object> obj, string wire) in tests)
        {
            Dictionary<string, object> deserialized = Helpers.Deserialize(serializer, s_ctx, wire)!;
            Assert.AreEqual(obj, deserialized);
            string serialized = Helpers.Serialize(serializer, s_ctx, deserialized);
            Assert.AreEqual(wire, serialized);
        }
    }

    [Test]
    public void RoundTripEmptyObject()
    {
        var serializer = Serializer.Generate<Dictionary<string, object>>(s_ctx);

        const string wire = "{}";
        var list = new Dictionary<string, object>();

        var deserialized = Helpers.Deserialize(serializer, s_ctx, wire);
        Assert.AreEqual(list, deserialized);

        string serialized = Helpers.Serialize(serializer, s_ctx, deserialized);
        Assert.AreEqual(wire, serialized);
    }

    [Test]
    public void RoundTripObject()
    {
        const string given = @"
                             {
                                ""aString"": ""foo"",
                                ""anObject"": { ""baz"": ""luhrmann"" },
                                ""anInt"": { ""@int"": ""2147483647"" },
                                ""aLong"":{ ""@long"": ""9223372036854775807"" },
                                ""aDouble"":{ ""@double"": ""3.14159"" },
                                ""aDate"":{ ""@date"": ""2023-12-03"" },
                                ""aTime"":{ ""@time"": ""2023-12-03T14:52:10.0010010Z"" },
                                ""true"": true,
                                ""false"": false,
                                ""null"": null
                             }
                             ";

        var inner = new Dictionary<string, object>
        {
            { "baz",  "luhrmann" }
        };

        var expected = new Dictionary<string, object?>
        {
            { "aString", "foo" },
            { "anObject", inner },
            { "anInt", 2147483647 },
            { "aLong", 9223372036854775807 },
            { "aDouble", 3.14159d },
            { "aDate", new DateOnly(2023, 12, 3) },
            { "aTime", new DateTime(2023, 12, 3, 14, 52, 10, 1, DateTimeKind.Utc).AddTicks(10).ToLocalTime() },
            { "true", true },
            { "false", false },
            { "null", null }
        };

        var serializer = Serializer.Generate<Dictionary<string, object>>(s_ctx);

        var deserialized = Helpers.Deserialize(serializer, s_ctx, given);
        Assert.AreEqual(expected, deserialized);

        string serialized = Helpers.Serialize(serializer, s_ctx, deserialized);
        Assert.AreEqual(Regex.Replace(given, @"\s+", ""), serialized);
    }

    [Test]
    public void RoundTripDeserializeEscapedObject()
    {
        const string given = @"
                             {
                                ""@object"": {
                                    ""@int"": ""notanint"",
                                    ""anInt"": { ""@int"": ""123"" },
                                    ""@object"": ""notanobject"",
                                    ""anEscapedObject"": { ""@object"": { ""@long"": ""notalong"" } }
                                }
                             }
                             ";

        var inner = new Dictionary<string, object>
        {
            { "@long",  "notalong" }
        };

        var expected = new Dictionary<string, object>
        {
            { "@int", "notanint" },
            { "anInt", 123 },
            { "@object", "notanobject" },
            { "anEscapedObject", inner }

        };

        var serializer = Serializer.Generate<Dictionary<string, object>>(s_ctx);

        var deserialized = Helpers.Deserialize(serializer, s_ctx, given);
        Assert.AreEqual(expected, deserialized);

        string serialized = Helpers.Serialize(serializer, s_ctx, deserialized);
        Assert.AreEqual(Regex.Replace(given, @"\s+", ""), serialized);
    }

    [Test]
    public void RoundTripDictionaryWithValueType()
    {
        const string given = @"{
    ""k1"": { ""@int"": ""1"" },
    ""k2"": { ""@int"": ""2"" },
    ""k3"": { ""@int"": ""3"" }
    }";
        var expected = new Dictionary<string, int>()
            {
                {"k1", 1},
                {"k2", 2},
                {"k3", 3}
            };

        var serializer = Serializer.Generate<Dictionary<string, int>>(s_ctx);

        var deserialized = Helpers.Deserialize(serializer, s_ctx, given);
        Assert.AreEqual(expected, deserialized);

        string serialized = Helpers.Serialize(serializer, s_ctx, deserialized);
        Assert.AreEqual(Regex.Replace(given, @"\s+", ""), serialized);
    }
}
