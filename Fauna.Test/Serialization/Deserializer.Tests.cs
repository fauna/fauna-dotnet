using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

[TestFixture]
public class DeserializerTests
{
    private readonly MappingContext ctx;
    public DeserializerTests()
    {
        var colls = new Dictionary<string, Type> {
            { "MappedColl", typeof(ClassForDocument) }
        };
        ctx = new(colls);
    }

    public object? Deserialize(string str) =>
       DeserializeImpl(str, ctx => SerializerProvider.Dynamic);

    public T Deserialize<T>(string str) where T : notnull =>
       DeserializeImpl(str, ctx => SerializerProvider.Generate<T>(ctx));

    public T? DeserializeNullable<T>(string str) =>
        DeserializeImpl(str, ctx => SerializerProvider.GenerateNullable<T>(ctx));

    public T DeserializeImpl<T>(string str, Func<MappingContext, ISerializer<T>> deserFunc)
    {
        var reader = new Utf8FaunaReader(str);
        reader.Read();
        var deser = deserFunc(ctx);
        var obj = deser.Deserialize(ctx, ref reader);

        if (reader.Read())
        {
            throw new SerializationException($"Token stream is not exhausted but should be: {reader.CurrentTokenType}");
        }

        return obj;
    }

    private void RunTestCases<T>(Dictionary<string, T> cases) where T : notnull
    {
        foreach (KeyValuePair<string, T> entry in cases)
        {
            var result = Deserialize<T>(entry.Key);
            Assert.AreEqual(entry.Value, result);
        }
    }

    [Test]
    public void CastDeserializer()
    {
        var deser = SerializerProvider.Generate<string>(ctx);
        // should cast w/o failing due to covariance.
        var obj = (ISerializer<object?>)deser;

        Assert.AreEqual(deser, obj);
    }

    [Test]
    public void DeserializeValuesDynamic()
    {
        var tests = new Dictionary<string, object?>
        {
            {"\"hello\"", "hello"},
            {@"{""@int"":""42""}", 42},
            {@"{""@long"":""42""}", 42L},
            {@"{""@double"": ""1.2""}", 1.2d},
            {@"{""@date"": ""2023-12-03""}", new DateOnly(2023, 12, 3)},
            {@"{""@time"": ""2023-12-03T05:52:10.000001-09:00""}", new DateTime(2023, 12, 3, 14, 52, 10, 0, DateTimeKind.Utc).AddTicks(10).ToLocalTime()},
            {"true", true},
            {"false", false},
            {"null", null},
        };

        foreach (KeyValuePair<string, object?> entry in tests)
        {
            var result = Deserialize(entry.Key);
            Assert.AreEqual(entry.Value, result);
        }
    }

    [Test]
    public void DeserializeNullableGeneric()
    {
        var result = DeserializeNullable<string>("null");
        Assert.IsNull(result);
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

        var actual = Deserialize(given);
        switch (actual)
        {
            case NonNullDocument<Document> d:
                Assert.AreEqual("123", d.Value!.Id);
                Assert.AreEqual("MyColl", d.Value!.Collection.Name);
                Assert.AreEqual(DateTime.Parse("2023-12-15T01:01:01.0010010Z"), d.Value!.Ts);
                Assert.AreEqual("name_value", d.Value!["name"]);
                break;
            default:
                Assert.Fail($"result is type: {actual?.GetType()}");
                break;
        }
    }

    [Test]
    public void DeserializeNullDocument()
    {
        const string given = @"
                             {
                                 ""@ref"":{
                                     ""id"":""123"",
                                     ""coll"":{""@mod"":""MyColl""},
                                     ""exists"":false,
                                     ""cause"":""not found""
                                 }
                             }";

        var actual = Deserialize(given);
        switch (actual)
        {
            case NullDocument<Document> d:
                Assert.AreEqual("123", d.Id);
                Assert.AreEqual("MyColl", d.Collection.Name);
                Assert.AreEqual("not found", d.Cause);
                break;
            default:
                Assert.Fail($"result is type: {actual?.GetType()}");
                break;
        }
    }

    [Test]
    public void DeserializeDocumentGeneric()
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
    public void DeserializeNonNullDocumentGeneric()
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

        var actual = Deserialize<NullableDocument<Document>>(given);
        switch (actual)
        {
            case NonNullDocument<Document> d:
                Assert.AreEqual("123", d.Value!.Id);
                Assert.AreEqual(new Module("MyColl"), d.Value!.Collection);
                Assert.AreEqual(DateTime.Parse("2023-12-15T01:01:01.0010010Z"), d.Value!.Ts);
                Assert.AreEqual("name_value", d.Value!["name"]);
                break;
            default:
                Assert.Fail($"result is type: {actual.GetType()}");
                break;
        }
    }

    [Test]
    public void DeserializeNullDocumentGeneric()
    {
        const string given = @"
                             {
                                 ""@ref"":{
                                     ""id"":""123"",
                                     ""coll"":{""@mod"":""MyColl""},
                                     ""exists"":false,
                                     ""cause"":""not found""
                                 }
                             }";

        var actual = Deserialize<NullableDocument<Document>>(given);

        switch (actual)
        {
            case NullDocument<Document> d:
                Assert.AreEqual("123", d.Id);
                Assert.AreEqual("MyColl", d.Collection.Name);
                Assert.AreEqual("not found", d.Cause);
                break;
            default:
                Assert.Fail($"result is type: {actual.GetType()}");
                break;
        }
    }

    [Test]
    public void DeserializeNullDocumentGenericThrowsWithoutWrapper()
    {
        const string given = @"
                             {
                                 ""@ref"":{
                                     ""id"":""123"",
                                     ""coll"":{""@mod"":""MyColl""},
                                     ""exists"":false,
                                     ""cause"":""not found""
                                 }
                             }";

        var e = Assert.Throws<NullDocumentException>(() => Deserialize<Document>(given));
        Assert.NotNull(e);
        Assert.AreEqual("123", e!.Id);
        Assert.AreEqual("MyColl", e.Collection.Name);
        Assert.AreEqual("not found", e.Cause);
        Assert.AreEqual("Document 123 in collection MyColl is null: not found", e.Message);
    }

    [Test]
    public void DeserializeDocumentToNonNullDocumentClass()
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

        var actual = Deserialize<NullableDocument<ClassForDocument>>(given);
        switch (actual)
        {
            case NonNullDocument<ClassForDocument> d:
                Assert.AreEqual("123", d.Value!.Id);
                Assert.AreEqual("user_value", d.Value!.UserField);
                break;
            default:
                Assert.Fail($"result is type: {actual?.GetType()}");
                break;
        }
    }

    [Test]
    public void DeserializeDocumentToNullDocumentClass()
    {
        const string given = @"
                             {
                                 ""@ref"":{
                                     ""id"":""123"",
                                     ""coll"":{""@mod"":""MyColl""},
                                     ""exists"":false,
                                     ""cause"":""not found""
                                 }
                             }";

        var actual = Deserialize<NullableDocument<ClassForDocument>>(given);
        switch (actual)
        {
            case NullDocument<ClassForDocument> d:
                Assert.AreEqual("123", d.Id);
                Assert.AreEqual("MyColl", d.Collection.Name);
                Assert.AreEqual("not found", d.Cause);
                break;
            default:
                Assert.Fail($"result is type: {actual?.GetType()}");
                break;
        }
    }

    [Test]
    public void DeserializeNullDocumentClassThrowsWithoutWrapper()
    {
        const string given = @"
                             {
                                 ""@ref"":{
                                     ""id"":""123"",
                                     ""coll"":{""@mod"":""MyColl""},
                                     ""exists"":false,
                                     ""cause"":""not found""
                                 }
                             }";

        var e = Assert.Throws<NullDocumentException>(() => Deserialize<ClassForDocument>(given));
        Assert.NotNull(e);
        Assert.AreEqual("123", e!.Id);
        Assert.AreEqual("MyColl", e.Collection.Name);
        Assert.AreEqual("not found", e.Cause);
        Assert.AreEqual("Document 123 in collection MyColl is null: not found", e.Message);
    }

    [Test]
    public void DeserializeDocumentToRegisteredClass()
    {
        const string given = @"
                             {
                                 ""@doc"":{
                                     ""id"":""123"",
                                     ""coll"":{""@mod"":""MappedColl""},
                                     ""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},
                                     ""user_field"":""user_value""
                                 }
                             }";

        if (Deserialize(given) is ClassForDocument actual)
        {
            Assert.AreEqual("user_value", actual.UserField);
            Assert.AreEqual("123", actual.Id);
        }
        else
        {
            Assert.Fail();
        }
    }

    [Test]
    public void DeserializeRegisteredClassToDocumentRef()
    {
        const string given = @"
                             {
                                 ""@doc"":{
                                     ""id"":""123"",
                                     ""coll"":{""@mod"":""MappedColl""},
                                     ""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},
                                     ""user_field"":""user_value""
                                 }
                             }";

        var actual = Deserialize<Ref>(given);
        Assert.AreEqual("123", actual.Id);
        Assert.AreEqual(new Module("MappedColl"), actual.Collection);
    }

    [Test]
    public void DeserializeRegisteredClassToDocument()
    {
        const string given = @"
                             {
                                 ""@doc"":{
                                     ""id"":""123"",
                                     ""coll"":{""@mod"":""MappedColl""},
                                     ""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},
                                     ""user_field"":""user_value""
                                 }
                             }";

        var actual = Deserialize<Document>(given);
        Assert.AreEqual("123", actual.Id);
        Assert.AreEqual(new Module("MappedColl"), actual.Collection);
        Assert.AreEqual(DateTime.Parse("2023-12-15T01:01:01.0010010Z"), actual.Ts);
        Assert.AreEqual("user_value", actual["user_field"]);
    }

    [Test]
    public void DeserializeNamedDocumentUnchecked()
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

        var actual = Deserialize(given);
        switch (actual)
        {
            case NamedDocument d:
                Assert.AreEqual("DocName", d.Name);
                Assert.AreEqual("MyColl", d.Collection.Name);
                Assert.AreEqual(DateTime.Parse("2023-12-15T01:01:01.0010010Z"), d.Ts);
                Assert.AreEqual("user_value", d["user_field"]);
                break;
            default:
                Assert.Fail($"result is type: {actual?.GetType()}");
                break;
        }
    }

    [Test]
    public void DeserializeNullNamedDocumentUnchecked()
    {
        const string given = @"
                             {
                                 ""@ref"":{
                                     ""name"":""RefName"",
                                     ""coll"":{""@mod"":""MyColl""},
                                     ""exists"":false,
                                     ""cause"":""not found""
                                 }
                             }";

        var actual = Deserialize(given);
        switch (actual)
        {
            case NullDocument<NamedDocument> d:
                Assert.AreEqual("RefName", d.Id);
                Assert.AreEqual("MyColl", d.Collection.Name);
                Assert.AreEqual("not found", d.Cause);
                break;
            default:
                Assert.Fail($"result is type: {actual?.GetType()}");
                break;
        }
    }

    [Test]
    public void DeserializeNamedDocumentChecked()
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

        var actual = Deserialize<NamedDocument>(given);
        Assert.AreEqual("DocName", actual.Name);
        Assert.AreEqual(new Module("MyColl"), actual.Collection);
        Assert.AreEqual(DateTime.Parse("2023-12-15T01:01:01.0010010Z"), actual.Ts);
        Assert.AreEqual("user_value", actual["user_field"]);
    }

    [Test]
    public void DeserializeNullNamedDocumentChecked()
    {
        const string given = @"
                             {
                                 ""@ref"":{
                                     ""name"":""RefName"",
                                     ""coll"":{""@mod"":""MyColl""},
                                     ""exists"":false,
                                     ""cause"":""not found""
                                 }
                             }";

        var actual = Deserialize<NullableDocument<NamedDocument>>(given);

        switch (actual)
        {
            case NullDocument<NamedDocument> d:
                Assert.AreEqual("RefName", d.Id);
                Assert.AreEqual("MyColl", d.Collection.Name);
                Assert.AreEqual("not found", d.Cause);
                break;
            default:
                Assert.Fail($"result is type: {actual.GetType()}");
                break;
        }
    }

    [Test]
    public void DeserializeNullNamedDocumentThrowsWithoutWrapper()
    {
        const string given = @"
                             {
                                 ""@ref"":{
                                     ""name"":""RefName"",
                                     ""coll"":{""@mod"":""MyColl""},
                                     ""exists"":false,
                                     ""cause"":""not found""
                                 }
                             }";

        var e = Assert.Throws<NullDocumentException>(() => Deserialize<NamedDocument>(given));
        Assert.NotNull(e);
        Assert.AreEqual("RefName", e!.Id);
        Assert.AreEqual("MyColl", e.Collection.Name);
        Assert.AreEqual("not found", e.Cause);
        Assert.AreEqual("Document RefName in collection MyColl is null: not found", e.Message);
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

        var actual = Deserialize<ClassForNamedDocument>(given);
        Assert.AreEqual("DocName", actual.Name);
        Assert.AreEqual("user_value", actual.UserField);
    }

    [Test]
    public void DeserializeNonNullNamedDocumentToClass()
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

        var actual = Deserialize<NullableDocument<ClassForNamedDocument>>(given);
        switch (actual)
        {
            case NonNullDocument<ClassForNamedDocument> d:
                Assert.AreEqual("DocName", d.Value!.Name);
                Assert.AreEqual("user_value", d.Value!.UserField);
                break;
            default:
                Assert.Fail($"result is type: {actual?.GetType()}");
                break;
        }
    }

    [Test]
    public void DeserializeNullNamedDocumentToClass()
    {
        const string given = @"
                             {
                                 ""@ref"":{
                                     ""name"":""RefName"",
                                     ""coll"":{""@mod"":""MyColl""},
                                     ""exists"":false,
                                     ""cause"":""not found""
                                 }
                             }";

        var actual = Deserialize<NullableDocument<ClassForNamedDocument>>(given);
        switch (actual)
        {
            case NullDocument<ClassForNamedDocument> d:
                Assert.AreEqual("RefName", d.Id);
                Assert.AreEqual("MyColl", d.Collection.Name);
                Assert.AreEqual("not found", d.Cause);
                break;
            default:
                Assert.Fail($"result is type: {actual?.GetType()}");
                break;
        }
    }

    [Test]
    public void DeserializeNullNamedDocumentClassThrowsWithoutWrapper()
    {
        const string given = @"
                             {
                                 ""@ref"":{
                                     ""name"":""RefName"",
                                     ""coll"":{""@mod"":""MyColl""},
                                     ""exists"":false,
                                     ""cause"":""not found""
                                 }
                             }";

        var e = Assert.Throws<NullDocumentException>(() => Deserialize<ClassForNamedDocument>(given));
        Assert.NotNull(e);
        Assert.AreEqual("RefName", e!.Id);
        Assert.AreEqual("MyColl", e.Collection.Name);
        Assert.AreEqual("not found", e.Cause);
        Assert.AreEqual("Document RefName in collection MyColl is null: not found", e.Message);
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
    public void DeserializeDocumentAsDocumentRef()
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

        var actual = Deserialize<NamedRef>(given);
        Assert.AreEqual("RefName", actual.Name);
        Assert.AreEqual(new Module("MyColl"), actual.Collection);
    }

    [Test]
    public void DeserializeNamedDocumentAsNamedDocumentRef()
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

        var actual = Deserialize<NamedRef>(given);
        Assert.AreEqual("DocName", actual.Name);
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
            { "aDate", new DateOnly(2023, 12, 3) },
            { "aTime", new DateTime(2023, 12, 3, 14, 52, 10, 1, DateTimeKind.Utc).AddTicks(10).ToLocalTime() },
            { "true", true },
            { "false", false },
            { "null", null }
        };

        var result = Deserialize(given);
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

        var result = Deserialize(given);
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
                                ""firstName"": ""Baz2"",
                                ""lastName"": ""Luhrmann2"",
                                ""age"": { ""@int"": ""612"" }
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
                                ""ignored"": ""should be null""
                             }
                             ";

        var p = Deserialize<PersonWithAttributes>(given);
        Assert.AreEqual("Baz2", p.FirstName);
        Assert.AreEqual("Luhrmann2", p.LastName);
        Assert.AreEqual(612, p.Age);
        Assert.IsNull(p.Ignored);
    }

    [Test]
    public void DeserializeIntoPageWithPrimitive()
    {
        const string given = @"{
            ""@set"": {
                ""after"": ""next_page_cursor"",
                ""data"": [
                    {""@int"":""1""},
                    {""@int"":""2""},
                    {""@int"":""3""}
                ]
            }
        }";

        var result = Deserialize<Page<int>>(given);
        Assert.IsNotNull(result);
        Assert.AreEqual(new List<int> { 1, 2, 3 }, result.Data);
        Assert.AreEqual("next_page_cursor", result.After);
    }

    [Test]
    public void DeserializeIntoPageWithSingleValue()
    {
        const string given = @"""SingleValue""";

        var result = Deserialize<Page<string>>(given);
        Assert.IsNotNull(result);
        Assert.AreEqual(new List<string> { "SingleValue" }, result.Data);
        Assert.IsNull(result.After);
    }

    [Test]
    public void DeserializeIntoPageWithUserDefinedClass()
    {
        const string given = @"{
            ""@set"": {
                ""after"": ""next_page_cursor"",
                ""data"": [
                    {""first_name"":""Alice"",""last_name"":""Smith"",""age"":{""@int"":""30""}},
                    {""first_name"":""Bob"",""last_name"":""Jones"",""age"":{""@int"":""40""}}
                ]
            }
        }";

        var result = Deserialize<Page<PersonWithAttributes>>(given);
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Data.Count);
        Assert.AreEqual("Alice", result.Data[0].FirstName);
        Assert.AreEqual("Smith", result.Data[0].LastName);
        Assert.AreEqual(30, result.Data[0].Age);
        Assert.AreEqual("Bob", result.Data[1].FirstName);
        Assert.AreEqual("Jones", result.Data[1].LastName);
        Assert.AreEqual(40, result.Data[1].Age);
        Assert.AreEqual("next_page_cursor", result.After);
    }

    [Test]
    public void DeserializeNullableStructAsValue()
    {
        const string given = @"{
            ""val"":{""@int"":""42""}
        }";

        var result = Deserialize<NullableInt>(given);
        Assert.IsNotNull(result);
        Assert.AreEqual(42, result.Val);
    }

    [Test]
    public void DeserializeNullableStructAsNull()
    {
        const string given = @"{
            ""val"": null
        }";

        var result = Deserialize<NullableInt>(given);
        Assert.IsNotNull(result);
        Assert.AreEqual(null, result.Val);
    }
}
