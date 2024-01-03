using Fauna.Serialization;
using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

[TestFixture]
public partial class SerializerTests
{
    static T Deserialize<T>(string str) => (T)Deserialize(str, typeof(T))!;

    static object? Deserialize(string str, Type? type)
    {
        var reader = new Utf8FaunaReader(str);
        reader.Read();
        var context = new SerializationContext();
        var obj = Serializer.Deserialize(context, ref reader, type);

        if (reader.Read())
        {
            throw new SerializationException($"Token stream is not exhausted but should be: {reader.CurrentTokenType}");
        }

        return obj;
    }

    [Test]
    public void DeserializeValues()
    {
        var tests = new Dictionary<string, object?>
        {
            {"\"hello\"", "hello"},
            {@"{""@int"":""42""}", 42},
            {@"{""@long"":""42""}", 42L},
            {@"{""@double"": ""1.2""}", 1.2d},
            {@"{""@date"": ""2023-12-03""}", new DateTime(2023, 12, 3)},
            {@"{""@time"": ""2023-12-03T05:52:10.000001-09:00""}", new DateTime(2023, 12, 3, 14, 52, 10, 0, DateTimeKind.Utc).AddTicks(10).ToLocalTime()},
            {"true", true},
            {"false", false},
            {"null", null},
        };

        foreach (KeyValuePair<string, object?> entry in tests)
        {
            var result = Deserialize(entry.Key, null);
            Assert.AreEqual(entry.Value, result);
        }
    }

    [Test]
    public void DeserializeStringGeneric()
    {
        var result = Deserialize<string>("\"hello\"");
        Assert.AreEqual("hello", result);
    }

    [Test]
    public void DeserializeNullableGeneric()
    {
        var result = Deserialize<string?>("null");
        Assert.IsNull(result);
    }

    [Test]
    public void DeserializeIntGeneric()
    {
        var result = Deserialize<int>(@"{""@int"":""42""}");
        Assert.AreEqual(42, result);
    }


    [Test]
    public void DeserializeDateGeneric()
    {
        var result = Deserialize<DateTime>(@"{""@date"": ""2023-12-03""}");
        Assert.AreEqual(new DateTime(2023, 12, 3), result);
    }

    [Test]
    public void DeserializeTimeGeneric()
    {
        var result = Deserialize<DateTime>(@"{""@time"": ""2023-12-03T05:52:10.000001-09:00""}");
        var expected = new DateTime(2023, 12, 3, 14, 52, 10, 0, DateTimeKind.Utc).AddTicks(10).ToLocalTime();
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void DeserializeDocument()
    {
        const string given = @"
                             {
                                 ""@doc"":{
                                     ""id"":""123"",
                                     ""coll"":{""@mod"":""MyColl""},
                                     ""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},
                                     ""name"":""name_value""
                                 }
                             }";

        var actual = Serializer.Deserialize(given);
        Assert.AreEqual(typeof(Document), actual?.GetType());
        var typed = (actual as Document)!;
        Assert.AreEqual("123", typed.Id);
        Assert.AreEqual(new Module("MyColl"), typed.Collection);
        Assert.AreEqual(DateTime.Parse("2023-12-15T01:01:01.0010010Z"), typed.Ts);
        Assert.AreEqual("name_value", typed["name"]);
    }

    [Test]
    public void DeserializeDocumentWithType()
    {
        const string given = @"
                             {
                                 ""@doc"":{
                                     ""id"":""123"",
                                     ""coll"":{""@mod"":""MyColl""},
                                     ""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},
                                     ""name"":""name_value""
                                 }
                             }";

        var actual = Deserialize<Document>(given);
        Assert.AreEqual("123", actual.Id);
        Assert.AreEqual(new Module("MyColl"), actual.Collection);
        Assert.AreEqual(DateTime.Parse("2023-12-15T01:01:01.0010010Z"), actual.Ts);
        Assert.AreEqual("name_value", actual["name"]);
    }

    [Test]
    public void DeserializeDocumentToClass()
    {
        const string given = @"
                             {
                                 ""@doc"":{
                                     ""id"":""123"",
                                     ""coll"":{""@mod"":""MyColl""},
                                     ""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},
                                     ""user_field"":""user_value""
                                 }
                             }";

        var actual = Deserialize<ClassForDocument>(given);
        Assert.AreEqual("user_value", actual.UserField);
    }

    [Test]
    public void DeserializeNamedDocument()
    {
        const string given = @"
                             {
                                 ""@doc"":{
                                     ""name"":""DocName"",
                                     ""coll"":{""@mod"":""MyColl""},
                                     ""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},
                                     ""user_field"":""user_value""
                                 }
                             }";

        var actual = Serializer.Deserialize(given);
        Assert.AreEqual(typeof(NamedDocument), actual?.GetType());
        var typed = (actual as NamedDocument)!;
        Assert.AreEqual("DocName", typed.Name);
        Assert.AreEqual(new Module("MyColl"), typed.Collection);
        Assert.AreEqual(DateTime.Parse("2023-12-15T01:01:01.0010010Z"), typed.Ts);
        Assert.AreEqual("user_value", typed["user_field"]);
    }

    [Test]
    public void DeserializeNamedDocumentWithType()
    {
        const string given = @"
                             {
                                 ""@doc"":{
                                     ""name"":""DocName"",
                                     ""coll"":{""@mod"":""MyColl""},
                                     ""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},
                                     ""user_field"":""user_value""
                                 }
                             }";

        var actual = Serializer.Deserialize<NamedDocument>(given);
        Assert.AreEqual("DocName", actual.Name);
        Assert.AreEqual(new Module("MyColl"), actual.Collection);
        Assert.AreEqual(DateTime.Parse("2023-12-15T01:01:01.0010010Z"), actual.Ts);
        Assert.AreEqual("user_value", actual["user_field"]);
    }

    [Test]
    public void DeserializeNamedDocumentToClass()
    {
        const string given = @"
                             {
                                 ""@doc"":{
                                     ""name"":""DocName"",
                                     ""coll"":{""@mod"":""MyColl""},
                                     ""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},
                                     ""user_field"":""user_value""
                                 }
                             }";

        var actual = Serializer.Deserialize<ClassForNamedDocument>(given);
        Assert.AreEqual("DocName", actual.Name);
        Assert.AreEqual("user_value", actual.UserField);
    }

    [Test]
    public void DeserializeRef()
    {
        const string given = @"
                             {
                                 ""@ref"":{
                                     ""id"":""123"",
                                     ""coll"":{""@mod"":""MyColl""}
                                 }
                             }";

        var actual = Deserialize<Ref>(given);
        Assert.AreEqual("123", actual.Id);
        Assert.AreEqual(new Module("MyColl"), actual.Collection);
    }

    [Test]
    public void DeserializeNamedRef()
    {
        const string given = @"
                             {
                                 ""@ref"":{
                                     ""name"":""RefName"",
                                     ""coll"":{""@mod"":""MyColl""}
                                 }
                             }";

        var actual = Serializer.Deserialize<NamedRef>(given);
        Assert.AreEqual("RefName", actual.Name);
        Assert.AreEqual(new Module("MyColl"), actual.Collection);
    }


    [Test]
    public void DeserializeObject()
    {
        const string given = @"
                             {
                                ""aString"": ""foo"",
                                ""anObject"": { ""baz"": ""luhrmann"" },
                                ""anInt"": { ""@int"": ""2147483647"" },
                                ""aLong"":{ ""@long"": ""9223372036854775807"" },
                                ""aDouble"":{ ""@double"": ""3.14159"" },
                                ""aDate"":{ ""@date"": ""2023-12-03"" },
                                ""aTime"":{ ""@time"": ""2023-12-03T14:52:10.001001Z"" },
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
            { "aDate", new DateTime(2023, 12, 3) },
            { "aTime", new DateTime(2023, 12, 3, 14, 52, 10, 1, DateTimeKind.Utc).AddTicks(10).ToLocalTime() },
            { "true", true },
            { "false", false },
            { "null", null }
        };

        var result = Deserialize(given, null);
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void DeserializeEscapedObject()
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

        var result = Deserialize(given, null);
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void DeserializeIntoGenericDictionary()
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
        var actual = Deserialize<Dictionary<string, int>>(given);
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void DeserializeIntoPoco()
    {

        const string given = @"
                             {
                                ""FirstName"": ""Baz2"",
                                ""LastName"": ""Luhrmann2"",
                                ""Age"": { ""@int"": ""612"" }
                             }
                             ";

        var p = Deserialize<Person>(given);
        Assert.AreEqual("Baz2", p.FirstName);
        Assert.AreEqual("Luhrmann2", p.LastName);
        Assert.AreEqual(612, p.Age);
    }

    [Test]
    public void DeserializeIntoPocoWithAttributes()
    {
        const string given = @"
                             {
                                ""first_name"": ""Baz2"",
                                ""last_name"": ""Luhrmann2"",
                                ""age"": { ""@int"": ""612"" },
                                ""Ignored"": ""should be null""
                             }
                             ";

        var p = Deserialize<PersonWithAttributes>(given);
        Assert.AreEqual("Baz2", p.FirstName);
        Assert.AreEqual("Luhrmann2", p.LastName);
        Assert.AreEqual(612, p.Age);
        Assert.IsNull(p.Ignored);
    }

    [Test]
    public void DeserializeIntoList()
    {
        const string given = @"[""item1"",""item2""]";
        var expected = new List<object> { "item1", "item2" };
        var p = Deserialize(given, null);
        Assert.AreEqual(expected, p);
    }

    [Test]
    public void DeserializeIntoGenericListWithPrimitive()
    {
        const string given = @"[""item1"",""item2""]";
        var expected = new List<string> { "item1", "item2" };
        var p = Deserialize<List<string>>(given);
        Assert.AreEqual(expected, p);
    }

    [Test]
    public void DeserializeIntoGenericListWithPocoWithAttributes()
    {
        const string given = @"[
{""first_name"":""Alice"",""last_name"":""Smith"",""age"":{""@int"":""100""}},
{""first_name"":""Bob"",""last_name"":""Jones"",""age"":{""@int"":""101""}}
]";
        var peeps = Deserialize<List<PersonWithAttributes>>(given);
        var alice = peeps[0];
        var bob = peeps[1];
        Assert.AreEqual("Alice", alice.FirstName);
        Assert.AreEqual("Smith", alice.LastName);
        Assert.AreEqual(100, alice.Age);

        Assert.AreEqual("Bob", bob.FirstName);
        Assert.AreEqual("Jones", bob.LastName);
        Assert.AreEqual(101, bob.Age);
    }
}
