using System.Buffers;
using System.Text;
using Fauna.Serialization;
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
        Assert.AreEqual(TokenType.String, reader.TokenType);
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
        Assert.AreEqual(TokenType.True, reader.TokenType);
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
        Assert.AreEqual(TokenType.False, reader.TokenType);
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
        Assert.AreEqual(TokenType.Null, reader.TokenType);
        Assert.False(reader.Read());
    }
    
    [Test]
    public void ReadIntToken()
    {
        const string s = """{"@int": "123"}""";
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.Int, reader.TokenType);
        Assert.AreEqual(123, reader.GetInt());
        Assert.False(reader.Read());
    }
    
    [Test]
    public void ReadLongToken()
    {
        const string s = """{"@long": "123"}""";
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.Long, reader.TokenType);
        Assert.AreEqual(123L, reader.GetLong());
        Assert.False(reader.Read());
    }
    
    [Test]
    public void ReadDoubleToken()
    {
        const string s = """{"@double": "1.2"}""";
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.Double, reader.TokenType);
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
        Assert.AreEqual(TokenType.Double, reader.TokenType);
        Assert.AreEqual(1.2M, reader.GetDoubleAsDecimal());
        Assert.False(reader.Read());
    }

    [Test]
    public void ReadDocumentTokens()
    {
        const string s = """
                         {
                            "@doc": {
                                "id": "123",
                                "data": { "foo": "bar" }
                            }
                         }
                         """;
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.StartDocument, reader.TokenType);
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.TokenType);
        Assert.AreEqual("id", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.TokenType);
        Assert.AreEqual("123", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.TokenType);
        Assert.AreEqual("data", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.StartObject, reader.TokenType);
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.TokenType);
        Assert.AreEqual("foo", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.TokenType);
        Assert.AreEqual("bar", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.EndObject, reader.TokenType);
        reader.Read();
        Assert.AreEqual(TokenType.EndDocument, reader.TokenType);
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
                            "true": true,
                            "false": false,
                            "null": null
                         }
                         """;
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.StartObject, reader.TokenType);
        
        // String
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.TokenType);
        Assert.AreEqual("aString", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.TokenType);
        Assert.AreEqual("foo", reader.GetString());
        
        // Object
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.TokenType);
        Assert.AreEqual("anObject", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.StartObject, reader.TokenType);
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.TokenType);
        Assert.AreEqual("baz", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.TokenType);
        Assert.AreEqual("luhrmann", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.EndObject, reader.TokenType);

        // Integer
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.TokenType);
        Assert.AreEqual("anInt", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.Int, reader.TokenType);
        Assert.AreEqual(2147483647, reader.GetInt());
        
        // Long
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.TokenType);
        Assert.AreEqual("aLong", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.Long, reader.TokenType);
        Assert.AreEqual(9223372036854775807L, reader.GetLong());
        
        // Double
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.TokenType);
        Assert.AreEqual("aDouble", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.Double, reader.TokenType);
        Assert.AreEqual(3.14159d, reader.GetDouble());
        
        // Decimal
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.TokenType);
        Assert.AreEqual("aDecimal", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.Double, reader.TokenType);
        Assert.AreEqual(0.1M, reader.GetDoubleAsDecimal());
        
        // True
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.TokenType);
        Assert.AreEqual("true", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.True, reader.TokenType);
        Assert.AreEqual(true, reader.GetBoolean());
        
        // False
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.TokenType);
        Assert.AreEqual("false", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.False, reader.TokenType);
        Assert.AreEqual(false, reader.GetBoolean());
        
        // Null
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.TokenType);
        Assert.AreEqual("null", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.Null, reader.TokenType);
        
        reader.Read();
        Assert.AreEqual(TokenType.EndObject, reader.TokenType);
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