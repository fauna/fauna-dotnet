using System.Buffers;
using System.Text;
using Fauna.Serialization;
using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

[TestFixture]
public class Utf8FaunaReaderTests
{

    private static void AssertReader(Utf8FaunaReader reader, IEnumerable<Tuple<TokenType, object?>> tokens)
    {
        foreach (var (token, obj) in tokens)
        {
            reader.Read();
            Assert.AreEqual(token, reader.CurrentTokenType);

            switch (token)
            {
                case TokenType.FieldName:
                case TokenType.String:
                    Assert.AreEqual(obj, reader.GetString());
                    break;
                case TokenType.Int:
                    Assert.AreEqual(obj, reader.GetInt());
                    break;
                case TokenType.Long:
                    Assert.AreEqual(obj, reader.GetLong());
                    break;
                case TokenType.Double:
                    if (obj is decimal)
                    {
                        Assert.AreEqual(obj, reader.GetDoubleAsDecimal());
                    }
                    else
                    {
                        Assert.AreEqual(obj, reader.GetDouble());
                    }
                    break;
                case TokenType.Date:
                    Assert.AreEqual(obj, reader.GetDate());
                    break;
                case TokenType.Time:
                    Assert.AreEqual(obj, reader.GetTime());
                    break;
                case TokenType.True:
                case TokenType.False:
                    Assert.AreEqual(obj, reader.GetBoolean());
                    break;
                case TokenType.Module:
                    Assert.AreEqual(obj, reader.GetModule());
                    break;
                default:
                    Assert.Null(obj);
                    break;
            }
        }

        Assert.False(reader.Read());
    }


    [Test]
    public void ReadString()
    {
        var reader = new Utf8FaunaReader("\"hello\"");
        var expectedTokens = new List<Tuple<TokenType, object?>>()
        {
            new(TokenType.String, "hello"),
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadTrue()
    {
        var reader = new Utf8FaunaReader("true");
        var expectedTokens = new List<Tuple<TokenType, object?>>()
        {
            new(TokenType.True, true)
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadFalse()
    {
        var reader = new Utf8FaunaReader("false");
        var expectedTokens = new List<Tuple<TokenType, object?>>()
        {
            new(TokenType.False, false)
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadNull()
    {
        var reader = new Utf8FaunaReader("null");
        var expectedTokens = new List<Tuple<TokenType, object?>>()
        {
            new(TokenType.Null, null)
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadInt()
    {
        const string s = @"{""@int"": ""123""}";
        var reader = new Utf8FaunaReader(s);
        var expectedTokens = new List<Tuple<TokenType, object?>>()
        {
            new(TokenType.Int, 123)
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadLong()
    {
        const string s = @"{""@long"": ""123""}";
        var reader = new Utf8FaunaReader(s);
        var expectedTokens = new List<Tuple<TokenType, object?>>()
        {
            new(TokenType.Long, 123L)
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadDouble()
    {
        const string s = @"{""@double"": ""1.2""}";
        var reader = new Utf8FaunaReader(s);
        var expectedTokens = new List<Tuple<TokenType, object?>>()
        {
            new(TokenType.Double, 1.2d)
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadDoubleAsDecimal()
    {
        const string s = @"{""@double"": ""1.2""}";
        var reader = new Utf8FaunaReader(s);
        var expectedTokens = new List<Tuple<TokenType, object?>>()
        {
            new(TokenType.Double, 1.2M)
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadDate()
    {
        const string s = @"{""@date"": ""2023-12-03""}";
        var reader = new Utf8FaunaReader(s);
        var expectedTokens = new List<Tuple<TokenType, object?>>()
        {
            new(TokenType.Date, new DateTime(2023, 12, 3))
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadTimePacific()
    {
        const string s = @"{""@time"": ""2023-12-03T05:52:10.000001-09:00""}";
        var reader = new Utf8FaunaReader(s);
        var expectedTokens = new List<Tuple<TokenType, object?>>()
        {
            new(TokenType.Time, new DateTime(2023, 12, 3, 14, 52, 10, 0, DateTimeKind.Utc).AddTicks(10).ToLocalTime())
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadTimeUtc()
    {
        const string s = @"{""@time"": ""2023-12-03T14:52:10.000001Z""}";
        var reader = new Utf8FaunaReader(s);
        var expectedTokens = new List<Tuple<TokenType, object?>>()
        {
            new(TokenType.Time, new DateTime(2023, 12, 3, 14, 52, 10, 0, DateTimeKind.Utc).AddTicks(10).ToLocalTime())
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadModule()
    {
        const string s = @"{""@mod"": ""MyCollection""}";
        var reader = new Utf8FaunaReader(s);
        var expectedTokens = new List<Tuple<TokenType, object?>>()
        {
            new(TokenType.Module, new Module("MyCollection"))
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadArrayWithEmptyObject()
    {
        const string s = "[{}]";
        var reader = new Utf8FaunaReader(s);
        var expectedTokens = new List<Tuple<TokenType, object?>>
        {
            new(TokenType.StartArray, null),
            new(TokenType.StartObject, null),
            new(TokenType.EndObject, null),
            new(TokenType.EndArray, null),
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadEscapedObject()
    {
        const string s = @"
                         {
                            ""@object"": {
                                ""@int"": ""notanint"",
                                ""anInt"": { ""@int"": ""123"" },
                                ""@object"": ""notanobject"",
                                ""anEscapedObject"": { ""@object"": { ""@long"": ""notalong"" } }
                            }
                         }
                         ";
        var reader = new Utf8FaunaReader(s);
        var expectedTokens = new List<Tuple<TokenType, object?>>
        {
            new(TokenType.StartObject, null),
            new(TokenType.FieldName, "@int"),
            new(TokenType.String, "notanint"),
            new(TokenType.FieldName, "anInt"),
            new(TokenType.Int, 123),
            new(TokenType.FieldName, "@object"),
            new(TokenType.String, "notanobject"),
            new(TokenType.FieldName, "anEscapedObject"),
            new(TokenType.StartObject, null),
            new(TokenType.FieldName, "@long"),
            new(TokenType.String, "notalong"),
            new(TokenType.EndObject, null),
            new(TokenType.EndObject, null)
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadDocumentTokens()
    {
        const string s = @"
                         {
                            ""@doc"": {
                                ""id"": ""123"",
                                ""coll"": { ""@mod"": ""Coll"" },
                                ""ts"": { ""@time"": ""2023-12-03T16:07:23.111012Z"" },
                                ""data"": { ""foo"": ""bar"" }
                            }
                         }
                         ";
        var reader = new Utf8FaunaReader(s);
        var expectedTokens = new List<Tuple<TokenType, object?>>
        {
            new(TokenType.StartDocument, null),
            new(TokenType.FieldName, "id"),
            new(TokenType.String, "123"),
            new(TokenType.FieldName, "coll"),
            new(TokenType.Module, new Module("Coll")),
            new(TokenType.FieldName, "ts"),
            new(TokenType.Time, new DateTime(2023, 12, 03, 16, 07, 23, 111, DateTimeKind.Utc).AddTicks(120).ToLocalTime()),
            new(TokenType.FieldName, "data"),
            new(TokenType.StartObject, null),
            new(TokenType.FieldName, "foo"),
            new(TokenType.String, "bar"),
            new(TokenType.EndObject, null),
            new(TokenType.EndDocument, null)
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadSet()
    {
        const string s = @"
                         {
                            ""@set"": {
                                ""data"": [{""@int"": ""99""}],
                                ""after"": ""afterme""
                            }
                         }
                         ";
        var reader = new Utf8FaunaReader(s);
        var expectedTokens = new List<Tuple<TokenType, object?>>
        {
            new(TokenType.StartPage, null),
            new(TokenType.FieldName, "data"),
            new(TokenType.StartArray, null),
            new(TokenType.Int, 99),
            new(TokenType.EndArray, null),
            new(TokenType.FieldName, "after"),
            new(TokenType.String, "afterme"),
            new(TokenType.EndPage, null)
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadRef()
    {
        const string s = @"{""@ref"": {""id"": ""123"", ""coll"": {""@mod"": ""Col""}}}";

        var reader = new Utf8FaunaReader(s);
        var expectedTokens = new List<Tuple<TokenType, object?>>
        {
            new(TokenType.StartRef, null),
            new(TokenType.FieldName, "id"),
            new(TokenType.String, "123"),
            new(TokenType.FieldName, "coll"),
            new(TokenType.Module, new Module("Col")),
            new(TokenType.EndRef, null)
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadObjectTokens()
    {
        const string s = @"
                         {
                            ""aString"": ""foo"",
                            ""anObject"": { ""baz"": ""luhrmann"" },
                            ""anInt"": { ""@int"": ""2147483647"" },
                            ""aLong"":{ ""@long"": ""9223372036854775807"" },
                            ""aDouble"":{ ""@double"": ""3.14159"" },
                            ""aDecimal"":{ ""@double"": ""0.1"" },
                            ""aDate"":{ ""@date"": ""2023-12-03"" },
                            ""aTime"":{ ""@time"": ""2023-12-03T14:52:10.001001Z"" },
                            ""anEscapedObject"": { ""@object"": { ""@int"": ""escaped"" } },
                            ""anArray"": [],
                            ""true"": true,
                            ""false"": false,
                            ""null"": null
                         }
                         ";

        var reader = new Utf8FaunaReader(s);
        var expectedTokens = new List<Tuple<TokenType, object?>>
        {
            new(TokenType.StartObject, null),

            new(TokenType.FieldName, "aString"),
            new(TokenType.String, "foo"),

            new(TokenType.FieldName, "anObject"),
            new(TokenType.StartObject, null),
            new(TokenType.FieldName, "baz"),
            new(TokenType.String, "luhrmann"),
            new(TokenType.EndObject, null),

            new(TokenType.FieldName, "anInt"),
            new(TokenType.Int, 2147483647),

            new(TokenType.FieldName, "aLong"),
            new(TokenType.Long, 9223372036854775807),

            new(TokenType.FieldName, "aDouble"),
            new(TokenType.Double, 3.14159d),

            new(TokenType.FieldName, "aDecimal"),
            new(TokenType.Double, 0.1M),

            new(TokenType.FieldName, "aDate"),
            new(TokenType.Date, new DateTime(2023, 12, 3)),

            new(TokenType.FieldName, "aTime"),
            new(TokenType.Time, new DateTime(2023, 12, 3, 14, 52, 10, 1, DateTimeKind.Utc).AddTicks(10).ToLocalTime()),

            new(TokenType.FieldName, "anEscapedObject"),
            new(TokenType.StartObject, null),
            new(TokenType.FieldName, "@int"),
            new(TokenType.String, "escaped"),
            new(TokenType.EndObject, null),

            new(TokenType.FieldName, "anArray"),
            new(TokenType.StartArray, null),
            new(TokenType.EndArray, null),

            new(TokenType.FieldName, "true"),
            new(TokenType.True, true),

            new(TokenType.FieldName, "false"),
            new(TokenType.False, false),

            new(TokenType.FieldName, "null"),
            new(TokenType.Null, null),

            new(TokenType.EndObject, null),
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ReadArray()
    {
        const string s = @"
                         [
                            ""foo"",
                            { ""baz"": ""luhrmann"" },
                            { ""@int"": ""2147483647"" },
                            { ""@long"": ""9223372036854775807"" },
                            { ""@double"": ""3.14159"" },
                            { ""@double"": ""0.1"" },
                            { ""@date"": ""2023-12-03"" },
                            { ""@time"": ""2023-12-03T14:52:10.001001Z"" },
                            { ""@object"": { ""@int"": ""escaped"" } },
                            [],
                            true,
                            false,
                            null
                         ]
                         ";

        var reader = new Utf8FaunaReader(s);

        var expectedTokens = new List<Tuple<TokenType, object?>>
        {
            new(TokenType.StartArray, null),

            new(TokenType.String, "foo"),

            new(TokenType.StartObject, null),
            new(TokenType.FieldName, "baz"),
            new(TokenType.String, "luhrmann"),
            new(TokenType.EndObject, null),

            new(TokenType.Int, 2147483647),

            new(TokenType.Long, 9223372036854775807),

            new(TokenType.Double, 3.14159d),

            new(TokenType.Double, 0.1M),

            new(TokenType.Date, new DateTime(2023, 12, 3)),

            new(TokenType.Time, new DateTime(2023, 12, 3, 14, 52, 10, 1, DateTimeKind.Utc).AddTicks(10).ToLocalTime()),

            new(TokenType.StartObject, null),
            new(TokenType.FieldName, "@int"),
            new(TokenType.String, "escaped"),
            new(TokenType.EndObject, null),

            new(TokenType.StartArray, null),
            new(TokenType.EndArray, null),

            new(TokenType.True, true),

            new(TokenType.False, false),

            new(TokenType.Null, null),

            new(TokenType.EndArray, null),
        };

        AssertReader(reader, expectedTokens);
    }

    [Test]
    public void ThrowsOnMalformedJson()
    {
        const string s = "{";
        var ex = Assert.Throws<SerializationException>(() =>
        {
            var reader = new Utf8FaunaReader(s);
            reader.Read();
            reader.Read();
        });

        Assert.AreEqual("Failed to advance underlying JSON reader.", ex?.Message);
    }

    [Test]
    public void SkipValues()
    {
        var tests = new List<string>()
        {
            @"{""k1"": {}, ""k2"": {}}",
            @"[""k1"",[],{}]",
            @"{""@ref"": {}}",
            @"{""@doc"": {}}",
            @"{""@set"": {}}",
            @"{""@object"":{}}"
        };

        foreach (var test in tests)
        {
            var reader = new Utf8FaunaReader(test);
            reader.Read();
            reader.Skip();
            Assert.IsFalse(reader.Read());
        }
    }

    [Test]
    public void SkipNestedEscapedObject()
    {
        const string test = @"{""@object"":{""inner"":{""@object"":{""foo"": ""bar""}},""k2"":{}}}";
        var reader = new Utf8FaunaReader(test);
        reader.Read(); // {"@object":{
        Assert.AreEqual(TokenType.StartObject, reader.CurrentTokenType);
        reader.Read(); // inner
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        reader.Read(); // {"@object":{
        Assert.AreEqual(TokenType.StartObject, reader.CurrentTokenType);
        reader.Skip(); // "foo": "bar"}}
        Assert.AreEqual(TokenType.EndObject, reader.CurrentTokenType);
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("k2", reader.GetString());
    }

    [Test]
    public void SkipNestedObject()
    {
        const string test = @"{""k"":{""inner"":{}},""k2"":{}}";
        var reader = new Utf8FaunaReader(test);
        reader.Read(); // {
        Assert.AreEqual(TokenType.StartObject, reader.CurrentTokenType);
        reader.Read(); // k
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        reader.Read(); // {
        Assert.AreEqual(TokenType.StartObject, reader.CurrentTokenType);
        reader.Skip(); // "inner":{}}
        Assert.AreEqual(TokenType.EndObject, reader.CurrentTokenType);
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("k2", reader.GetString());
    }

    [Test]
    public void SkipNestedArrays()
    {
        const string test = @"{""k"":[""1"",""2""],""k2"":{}}";
        var reader = new Utf8FaunaReader(test);
        reader.Read(); // {
        Assert.AreEqual(TokenType.StartObject, reader.CurrentTokenType);
        reader.Read(); // k
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        reader.Read(); // [
        Assert.AreEqual(TokenType.StartArray, reader.CurrentTokenType);
        reader.Skip(); // "1","2"]
        Assert.AreEqual(TokenType.EndArray, reader.CurrentTokenType);
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("k2", reader.GetString());
    }
}