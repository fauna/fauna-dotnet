using System.Buffers;
using System.Text;
using System.Text.Json;
using Fauna.Serialization;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

[TestFixture]
public class Utf8FaunaReaderTests
{

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
                            "foo": "bar",
                            "data": { "baz": "luhrmann" }
                         }
                         """;
        var bytes = Encoding.UTF8.GetBytes(s);
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(bytes));
        reader.Read();
        Assert.AreEqual(TokenType.StartObject, reader.TokenType);
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.TokenType);
        Assert.AreEqual("foo", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.String, reader.TokenType);
        Assert.AreEqual("bar", reader.GetString());
        reader.Read();
        Assert.AreEqual(TokenType.FieldName, reader.TokenType);
        Assert.AreEqual("data", reader.GetString());
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
        reader.Read();
        Assert.AreEqual(TokenType.EndObject, reader.TokenType);
        Assert.False(reader.Read());
    }
}