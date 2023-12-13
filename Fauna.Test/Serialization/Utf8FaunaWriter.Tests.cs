using System.Text;
using Fauna.Serialization;
using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

[TestFixture]
public class Utf8FaunaWriterTests
{

    private Utf8FaunaWriter _writer;
    private MemoryStream _stream;

    [SetUp]
    public void Init()
    {
        _stream = new MemoryStream();
        _writer = new Utf8FaunaWriter(_stream);
    }

    [TearDown]
    public void Cleanup()
    {
        _writer.Dispose();
    }

    private void AssertWriter(string expected)
    {
        _writer.Flush();
        var actual = Encoding.UTF8.GetString(_stream.ToArray());
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void WriteIntValue()
    {
        _writer.WriteIntValue(42);
        AssertWriter("""{"@int":"42"}""");
    }

    [Test]
    public void WriteLongValue()
    {
        _writer.WriteLongValue(42L);
        AssertWriter("""{"@long":"42"}""");
    }

    [Test]
    public void WriteDoubleValue()
    {
        _writer.WriteDoubleValue(1.2d);
        AssertWriter("""{"@double":"1.2"}""");
    }

    [Test]
    public void WriteTrueValue()
    {
        _writer.WriteBooleanValue(true);
        AssertWriter("true");
    }

    [Test]
    public void WriteFalseValue()
    {
        _writer.WriteBooleanValue(false);
        AssertWriter("false");
    }

    [Test]
    public void WriteNullValue()
    {
        _writer.WriteNullValue();
        AssertWriter("null");
    }

    [Test]
    public void WriteModuleValue()
    {
        _writer.WriteModuleValue(new Module("Authors"));
        AssertWriter("""{"@mod":"Authors"}""");
    }

    [Test]
    public void WriteDate()
    {
        var d = new DateTime(2023, 1, 1, 14, 04, 30, 1, 1, DateTimeKind.Utc);
        _writer.WriteDateValue(d);
        AssertWriter("""{"@date":"2023-01-01"}""");
    }

    [Test]
    public void WriteTime()
    {
        var d = new DateTime(2023, 1, 1, 14, 04, 30, 1, 1, DateTimeKind.Utc);
        _writer.WriteTimeValue(d);
        AssertWriter("""{"@time":"2023-01-01T14:04:30.0010010Z"}""");
    }

    [Test]
    public void WriteObject()
    {
        _writer.WriteStartObject();
        _writer.WriteInt("anInt", 42);
        _writer.WriteLong("aLong", 42L);
        _writer.WriteDouble("aDouble", 1.2d);
        _writer.WriteDouble("aDecimal", 3.14M);
        _writer.WriteBoolean("true", true);
        _writer.WriteBoolean("false", false);
        _writer.WriteString("foo", "bar");
        _writer.WriteDate("aDate", new DateTime(2023, 12, 4));
        _writer.WriteTime("aTime", new DateTime(2023, 12, 4, 0, 0, 0, 0, 0, DateTimeKind.Utc));
        _writer.WriteNull("aNull");
        _writer.WriteFieldName("anArray");
        _writer.WriteStartArray();
        _writer.WriteEndArray();
        _writer.WriteFieldName("anObject");
        _writer.WriteStartObject();
        _writer.WriteEndObject();
        _writer.WriteEndObject();
        AssertWriter("""{"anInt":{"@int":"42"},"aLong":{"@long":"42"},"aDouble":{"@double":"1.2"},"aDecimal":{"@double":"3.14"},"true":true,"false":false,"foo":"bar","aDate":{"@date":"2023-12-04"},"aTime":{"@time":"2023-12-04T00:00:00.0000000Z"},"aNull":null,"anArray":[],"anObject":{}}""");
    }

    [Test]
    public void WriteArray()
    {
        _writer.WriteStartArray();
        _writer.WriteIntValue(42);
        _writer.WriteLongValue(42L);
        _writer.WriteDoubleValue(1.2d);
        _writer.WriteDoubleValue(3.14M);
        _writer.WriteBooleanValue(true);
        _writer.WriteBooleanValue(false);
        _writer.WriteStringValue("bar");
        _writer.WriteDateValue(new DateTime(2023, 12, 4));
        _writer.WriteTimeValue(new DateTime(2023, 12, 4, 0, 0, 0, 0, 0, DateTimeKind.Utc));
        _writer.WriteNullValue();
        _writer.WriteStartArray();
        _writer.WriteEndArray();
        _writer.WriteStartObject();
        _writer.WriteEndObject();
        _writer.WriteEndArray();
        AssertWriter("""[{"@int":"42"},{"@long":"42"},{"@double":"1.2"},{"@double":"3.14"},true,false,"bar",{"@date":"2023-12-04"},{"@time":"2023-12-04T00:00:00.0000000Z"},null,[],{}]""");
    }

    [Test]
    public void WriteEscapedObject()
    {
        _writer.WriteStartEscapedObject();
        _writer.WriteEndEscapedObject();
        AssertWriter("""{"@object":{}}""");
    }

    [Test]
    public void WriteRef()
    {
        _writer.WriteStartRef();
        _writer.WriteString("id", "123");
        _writer.WriteModule("coll", new Module("Authors"));
        _writer.WriteEndRef();
        AssertWriter("""{"@ref":{"id":"123","coll":{"@mod":"Authors"}}}""");
    }
}
