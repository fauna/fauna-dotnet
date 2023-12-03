using System.Buffers;
using System.Text;
using Fauna.Serialization;
using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

[TestFixture]
public class Utf8FaunaReaderTests
{
    
    [Test]
    public void ReadString()
    {
        const string s = "\"hello\"";
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.CurrentTokenType);
        Assert.AreEqual("hello", reader.GetString());
        Assert.False(reader.Read());
    }
    
    [Test]
    public void ReadTrue()
    {
        const string s = "true";
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.True, reader.CurrentTokenType);
        Assert.AreEqual(true, reader.GetBoolean());
        Assert.False(reader.Read());
    }
    
    [Test]
    public void ReadFalse()
    {
        const string s = "false";
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.False, reader.CurrentTokenType);
        Assert.AreEqual(false, reader.GetBoolean());
        Assert.False(reader.Read());
    }
    
    [Test]
    public void ReadNull()
    {
        const string s = "null";
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.Null, reader.CurrentTokenType);
        Assert.False(reader.Read());
    }
    
    [Test]
    public void ReadInt()
    {
        const string s = """{"@int": "123"}""";
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.Int, reader.CurrentTokenType);
        Assert.AreEqual(123, reader.GetInt());
        Assert.False(reader.Read());
    }
    
    [Test]
    public void ReadLong()
    {
        const string s = """{"@long": "123"}""";
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.Long, reader.CurrentTokenType);
        Assert.AreEqual(123L, reader.GetLong());
        Assert.False(reader.Read());
    }
    
    [Test]
    public void ReadDouble()
    {
        const string s = """{"@double": "1.2"}""";
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.Double, reader.CurrentTokenType);
        Assert.AreEqual(1.2d, reader.GetDouble());
        Assert.False(reader.Read());
    }
    
    [Test]
    public void ReadDoubleAsDecimal()
    {
        const string s = """{"@double": "1.2"}""";
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.Double, reader.CurrentTokenType);
        Assert.AreEqual(1.2M, reader.GetDoubleAsDecimal());
        Assert.False(reader.Read());
    }

    [Test]
    public void ReadDate()
    {
        const string s = """{"@date": "2023-12-03"}""";
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.Date, reader.CurrentTokenType);
        Assert.AreEqual(new DateTime(2023, 12, 3), reader.GetDate());
        Assert.False(reader.Read());
    }

    [Test]
    public void ReadTimePacific()
    {
        const string s = """{"@time": "2023-12-03T05:52:10.000001-09:00"}""";
        var expected = new DateTime(2023, 12, 3, 14, 52, 10, 0, 1, DateTimeKind.Utc);
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.Time, reader.CurrentTokenType);
        var t = reader.GetTime();
        Assert.AreEqual(expected, reader.GetTime().ToUniversalTime());
        Assert.False(reader.Read());
    }
    
    [Test]
    public void ReadTimeUtc()
    {
        const string s = """{"@time": "2023-12-03T14:52:10.000001Z"}""";
        var expected = new DateTime(2023, 12, 3, 14, 52, 10, 0, 1, DateTimeKind.Utc);
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.Time, reader.CurrentTokenType);
        var t = reader.GetTime();
        Assert.AreEqual(expected, reader.GetTime().ToUniversalTime());
        Assert.False(reader.Read());
    }
    
    [Test]
    public void ReadModule()
    {
        const string s = """{"@mod": "MyCollection"}""";
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.Module, reader.CurrentTokenType);
        Assert.AreEqual(new Module("MyCollection"), reader.GetModule());
        Assert.False(reader.Read());
    }

    [Test]
    public void ReadEscapedObject()
    {
        const string s = """
                         {
                            "@object": { 
                                "@int": "notanint",
                                "anInt": { "@int": "123" },
                                "@object": "notanobject",
                                "anEscapedObject": { "@object": { "@long": "notalong" } }
                            }
                         }
                         """;
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.StartObject, reader.CurrentTokenType);
        
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("@int", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.CurrentTokenType);
        Assert.AreEqual("notanint", reader.GetString());
        
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("anInt", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.Int, reader.CurrentTokenType);
        Assert.AreEqual(123, reader.GetInt());
        
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("@object", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.CurrentTokenType);
        Assert.AreEqual("notanobject", reader.GetString());
        
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("anEscapedObject", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.StartObject, reader.CurrentTokenType);
        
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("@long", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.CurrentTokenType);
        Assert.AreEqual("notalong", reader.GetString());

        reader.Read();
        Assert.AreEqual(TokenType.EndObject, reader.CurrentTokenType);
        reader.Read();
        Assert.AreEqual(TokenType.EndObject, reader.CurrentTokenType);
        
        Assert.False(reader.Read());
    }

    [Test]
    public void ReadDocumentTokens()
    {
        const string s = """
                         {
                            "@doc": {
                                "id": "123",
                                "coll": { "@mod": "Coll" },
                                "ts": { "@time": "2023-12-03T16:07:23.111012Z" },
                                "data": { "foo": "bar" }
                            }
                         }
                         """;
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.StartDocument, reader.CurrentTokenType);
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("id", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.CurrentTokenType);
        Assert.AreEqual("123", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("coll", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.Module, reader.CurrentTokenType);
        Assert.AreEqual(new Module("Coll"), reader.GetModule());
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("ts", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.Time, reader.CurrentTokenType);
        Assert.AreEqual(
            new DateTime(2023, 12, 03, 16, 07, 23, 111, 12, DateTimeKind.Utc), 
            reader.GetTime().ToUniversalTime());
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("data", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.StartObject, reader.CurrentTokenType);
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("foo", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.CurrentTokenType);
        Assert.AreEqual("bar", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.EndObject, reader.CurrentTokenType);
        reader.Read();
        Assert.AreEqual(TokenType.EndDocument, reader.CurrentTokenType);
        Assert.False(reader.Read());
    }
    
    [Test]
    public void ReadSet()
    {
        const string s = """
                         {
                            "@set": {
                                "data": [{"@int": "99"}],
                                "after": "afterme"
                            }
                         }
                         """;
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.StartSet, reader.CurrentTokenType);
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("data", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.StartArray, reader.CurrentTokenType);
        reader.Read();
        Assert.AreEqual(TokenType.Int, reader.CurrentTokenType);
        Assert.AreEqual(99, reader.GetInt());
        reader.Read();
        Assert.AreEqual(TokenType.EndArray, reader.CurrentTokenType);
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("after", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.CurrentTokenType);
        Assert.AreEqual("afterme", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.EndSet, reader.CurrentTokenType);
        Assert.False(reader.Read());
    }
    
    [Test]
    public void ReadRef()
    {
        const string s = """{"@ref": {"id": "123", "coll": {"@mod": "Col"}}}""";
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        
        reader.Read();
        Assert.AreEqual(TokenType.StartRef, reader.CurrentTokenType);
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("id", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.CurrentTokenType);
        Assert.AreEqual("123", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("coll", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.Module, reader.CurrentTokenType);
        Assert.AreEqual(new Module("Col"), reader.GetModule());
        reader.Read();
        Assert.AreEqual(TokenType.EndRef, reader.CurrentTokenType);
        Assert.False(reader.Read());
    }
    
    [Test]
    public void ReadObjectTokens()
    {
        const string s = """
                         {
                            "aString": "foo",
                            "anObject": { "baz": "luhrmann" },
                            "anInt": { "@int": "2147483647" },
                            "aLong":{ "@long": "9223372036854775807" },
                            "aDouble":{ "@double": "3.14159" },
                            "aDecimal":{ "@double": "0.1" },
                            "aDate":{ "@date": "2023-12-03" },
                            "aTime":{ "@time": "2023-12-03T14:52:10.001001Z" },
                            "anObject": { "@object": { "@int": "escaped" } },
                            "anArray": [],
                            "true": true,
                            "false": false,
                            "null": null
                         }
                         """;
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.StartObject, reader.CurrentTokenType);
        
        // String
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("aString", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.CurrentTokenType);
        Assert.AreEqual("foo", reader.GetString());
        
        // Object
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("anObject", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.StartObject, reader.CurrentTokenType);
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("baz", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.CurrentTokenType);
        Assert.AreEqual("luhrmann", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.EndObject, reader.CurrentTokenType);

        // Integer
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("anInt", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.Int, reader.CurrentTokenType);
        Assert.AreEqual(2147483647, reader.GetInt());
        
        // Long
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("aLong", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.Long, reader.CurrentTokenType);
        Assert.AreEqual(9223372036854775807L, reader.GetLong());
        
        // Double
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("aDouble", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.Double, reader.CurrentTokenType);
        Assert.AreEqual(3.14159d, reader.GetDouble());
        
        // Decimal
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("aDecimal", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.Double, reader.CurrentTokenType);
        Assert.AreEqual(0.1M, reader.GetDoubleAsDecimal());
        
        // Date
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("aDate", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.Date, reader.CurrentTokenType);
        Assert.AreEqual(new DateTime(2023, 12, 3), reader.GetDate());
        
        // Time
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("aTime", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.Time, reader.CurrentTokenType);
        Assert.AreEqual(
            new DateTime(2023, 12, 3, 14, 52, 10, 1, 1, DateTimeKind.Utc),
            reader.GetTime().ToUniversalTime());
        
        // Escaped Object
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("anObject", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.StartObject, reader.CurrentTokenType);
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("@int", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.CurrentTokenType);
        Assert.AreEqual("escaped", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.EndObject, reader.CurrentTokenType);
        
        // Array
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("anArray", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.StartArray, reader.CurrentTokenType);
        reader.Read();
        Assert.AreEqual(TokenType.EndArray, reader.CurrentTokenType);
        
        // True
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("true", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.True, reader.CurrentTokenType);
        Assert.AreEqual(true, reader.GetBoolean());
        
        // False
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("false", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.False, reader.CurrentTokenType);
        Assert.AreEqual(false, reader.GetBoolean());
        
        // Null
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("null", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.Null, reader.CurrentTokenType);
        
        reader.Read();
        Assert.AreEqual(TokenType.EndObject, reader.CurrentTokenType);
        Assert.False(reader.Read());
    }
    
    [Test]
    public void ReadArray()
    {
        const string s = """
                         [
                            "foo",
                            { "baz": "luhrmann" },
                            { "@int": "2147483647" },
                            { "@long": "9223372036854775807" },
                            { "@double": "3.14159" },
                            { "@double": "0.1" },
                            { "@date": "2023-12-03" },
                            { "@time": "2023-12-03T14:52:10.001001Z" },
                            { "@object": { "@int": "escaped" } },
                            [],
                            true,
                            false,
                            null
                         ]
                         """;
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.StartArray, reader.CurrentTokenType);
        
        // String
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.CurrentTokenType);
        Assert.AreEqual("foo", reader.GetString());
        
        // Object
        reader.Read();
        Assert.AreEqual(TokenType.StartObject, reader.CurrentTokenType);
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("baz", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.CurrentTokenType);
        Assert.AreEqual("luhrmann", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.EndObject, reader.CurrentTokenType);

        // Integer
        reader.Read();
        Assert.AreEqual(TokenType.Int, reader.CurrentTokenType);
        Assert.AreEqual(2147483647, reader.GetInt());
        
        // Long
        reader.Read();
        Assert.AreEqual(TokenType.Long, reader.CurrentTokenType);
        Assert.AreEqual(9223372036854775807L, reader.GetLong());
        
        // Double
        reader.Read();
        Assert.AreEqual(TokenType.Double, reader.CurrentTokenType);
        Assert.AreEqual(3.14159d, reader.GetDouble());
        
        // Decimal
        reader.Read();
        Assert.AreEqual(TokenType.Double, reader.CurrentTokenType);
        Assert.AreEqual(0.1M, reader.GetDoubleAsDecimal());
        
        // Date
        reader.Read();
        Assert.AreEqual(TokenType.Date, reader.CurrentTokenType);
        Assert.AreEqual(new DateTime(2023, 12, 3), reader.GetDate());
        
        // Time
        reader.Read();
        Assert.AreEqual(TokenType.Time, reader.CurrentTokenType);
        Assert.AreEqual(
            new DateTime(2023, 12, 3, 14, 52, 10, 1, 1, DateTimeKind.Utc),
            reader.GetTime().ToUniversalTime());
        
        // Escaped Object
        reader.Read();
        Assert.AreEqual(TokenType.StartObject, reader.CurrentTokenType);
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.CurrentTokenType);
        Assert.AreEqual("@int", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.CurrentTokenType);
        Assert.AreEqual("escaped", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.EndObject, reader.CurrentTokenType);
        
        // Array
        reader.Read();
        Assert.AreEqual(TokenType.StartArray, reader.CurrentTokenType);
        reader.Read();
        Assert.AreEqual(TokenType.EndArray, reader.CurrentTokenType);
        
        // True
        reader.Read();
        Assert.AreEqual(TokenType.True, reader.CurrentTokenType);
        Assert.AreEqual(true, reader.GetBoolean());
        
        // False
        reader.Read();
        Assert.AreEqual(TokenType.False, reader.CurrentTokenType);
        Assert.AreEqual(false, reader.GetBoolean());
        
        // Null
        reader.Read();
        Assert.AreEqual(TokenType.Null, reader.CurrentTokenType);
        
        reader.Read();
        Assert.AreEqual(TokenType.EndArray, reader.CurrentTokenType);
        Assert.False(reader.Read());
    }

    [Test]
    public void ThrowsOnMalformedJson()
    {
        const string s = "{";
        var bytes = Encoding.UTF8.GetBytes(s);
        
        var ex = Assert.Throws<SerializationException>(() =>
        {
            var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
            reader.Read();
            reader.Read();
        });
        
        Assert.AreEqual("Failed to advance underlying JSON reader.", ex?.Message);
    }
}