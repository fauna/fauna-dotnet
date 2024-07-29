using System.Text;
using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

[TestFixture]
public class RoundTripTests
{
    private static readonly MappingContext ctx = new();
    private const string IntWire = @"{""@int"":""42""}";
    private const string LongWire = @"{""@long"":""42""}";
    private const string DoubleWire = @"{""@double"":""42""}";
    private const string DoubleWithDecimalWire = @"{""@double"":""42.2""}";
    private const string TrueWire = "true";
    private const string FalseWire = "false";
    private const string DateTimeWire = @"{""@time"":""2023-12-15T01:01:01.0010011Z""}";
    private const string DateWire = @"{""@date"":""2023-12-15""}";
    private const string ModuleWire = @"{""@mod"":""Foo""}";
    private const string DocumentWire = @"{""@doc"":{""id"":""123"",""coll"":{""@mod"":""MyColl""},""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},""user_field"":""user_value""}}";
    private const string DocumentRefWire = @"{""@ref"":{""id"":""123"",""coll"":{""@mod"":""MyColl""}}}";
    private const string NullDocumentWire = @"{""@ref"":{""id"":""123"",""coll"":{""@mod"":""MyColl""},""exists"":false,""cause"":""not found""}}";
    private const string NamedDocumentWire = @"{""@doc"":{""name"":""Foo"",""coll"":{""@mod"":""MyColl""},""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},""user_field"":""user_value""}}";
    private const string NullNamedDocumentWire = @"{""@ref"":{""name"":""Foo"",""coll"":{""@mod"":""MyColl""},""exists"":false,""cause"":""not found""}}";
    private const string NamedDocumentRefWire = @"{""@ref"":{""name"":""Foo"",""coll"":{""@mod"":""MyColl""}}}";
    private const string ObjectWithShortWire = @"{""a_short"":{""@int"":""42""}}";

    public static string Serialize(object? obj)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8FaunaWriter(stream);

        ISerializer ser = DynamicSerializer.Singleton;
        if (obj is not null) ser = Serializer.Generate(ctx, obj.GetType());
        ser.Serialize(ctx, writer, obj);

        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static T Deserialize<T>(string str) where T : notnull
    {
        var reader = new Utf8FaunaReader(str);
        reader.Read();
        var obj = Serializer.Generate<T>(ctx).Deserialize(ctx, ref reader);
        if (reader.Read())
        {
            throw new SerializationException($"Token stream is not exhausted but should be: {reader.CurrentTokenType}");
        }

        return obj;
    }

    [Test]
    public void RoundTripByte()
    {
        var deserialized = Deserialize<byte>(IntWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(IntWire, serialized);
    }

    [Test]
    public void RoundTripSByte()
    {
        var deserialized = Deserialize<sbyte>(IntWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(IntWire, serialized);
    }

    [Test]
    public void RoundTripShort()
    {
        var deserialized = Deserialize<short>(IntWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(IntWire, serialized);
    }

    [Test]
    public void RoundTripUShort()
    {
        var deserialized = Deserialize<ushort>(IntWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(IntWire, serialized);
    }

    [Test]
    public void RoundTripInt()
    {
        var deserialized = Deserialize<int>(IntWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(IntWire, serialized);
    }

    [Test]
    public void RoundTripUInt()
    {
        var deserialized = Deserialize<uint>(LongWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(LongWire, serialized);
    }

    [Test]
    public void RoundTripLong()
    {
        var deserialized = Deserialize<long>(LongWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(LongWire, serialized);
    }

    [Test]
    public void RoundTripFloat()
    {
        var deserialized = Deserialize<float>(DoubleWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(DoubleWire, serialized);
    }

    [Test]
    public void RoundTripDouble()
    {
        var deserialized = Deserialize<double>(DoubleWithDecimalWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(DoubleWithDecimalWire, serialized);
    }

    [Test]
    public void RoundTripTrue()
    {
        var deserialized = Deserialize<bool>(TrueWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(TrueWire, serialized);
    }

    [Test]
    public void RoundTripFalse()
    {
        var deserialized = Deserialize<bool>(FalseWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(FalseWire, serialized);
    }

    [Test]
    public void RoundTripDateTime()
    {
        var deserialized = Deserialize<DateTime>(DateTimeWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(DateTimeWire, serialized);
    }

    [Test]
    public void RoundTripDateOnly()
    {
        var deserialized = Deserialize<DateOnly>(DateWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(DateWire, serialized);
    }

    [Test]
    public void RoundTripModule()
    {
        var deserialized = Deserialize<Module>(ModuleWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(ModuleWire, serialized);
    }

    [Test]
    public void RoundTripClassWithShort()
    {
        var deserialized = Deserialize<ClassWithShort>(ObjectWithShortWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(ObjectWithShortWire, serialized);
    }

    [Test]
    public void RoundTripClassAsDocumentIsNotSupported()
    {
        var deserialized = Deserialize<ClassForDocument>(DocumentWire);
        var serialized = Serialize(deserialized);
        var expected = @"{""id"":{""@long"":""123""},""user_field"":""user_value""}";
        Assert.AreEqual(expected, serialized);
    }

    [Test]
    public void RoundTripDocumentRef()
    {
        var deserialized = Deserialize<Ref>(DocumentRefWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(DocumentRefWire, serialized);
    }

    [Test]
    public void RoundTripNonNullDocumentRef()
    {
        var deserialized = Deserialize<NullableDocument<Ref>>(DocumentRefWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(DocumentRefWire, serialized);
    }

    [Test]
    public void RoundTripNullDocumentRefChangesToDocumentRef()
    {
        var deserialized = Deserialize<NullableDocument<Ref>>(NullDocumentWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(DocumentRefWire, serialized);
    }

    [Test]
    public void RoundTripDocumentChangesToDocumentReference()
    {
        var deserialized = Deserialize<Document>(DocumentWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(DocumentRefWire, serialized);
    }

    [Test]
    public void RoundTripNonNullDocumentChangesToDocumentReference()
    {
        var deserialized = Deserialize<NullableDocument<Document>>(DocumentWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(DocumentRefWire, serialized);
    }

    [Test]
    public void RoundTripNullDocumentChangesToDocumentReference()
    {
        var deserialized = Deserialize<NullableDocument<Document>>(NullDocumentWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(DocumentRefWire, serialized);
    }

    [Test]
    public void RoundTripNamedDocumentRef()
    {
        var deserialized = Deserialize<NamedRef>(NamedDocumentRefWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(NamedDocumentRefWire, serialized);
    }

    [Test]
    public void RoundTripNonNullNamedDocumentRef()
    {
        var deserialized = Deserialize<NullableDocument<NamedRef>>(NamedDocumentRefWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(NamedDocumentRefWire, serialized);
    }

    [Test]
    public void RoundTripNullNamedDocumentRefChangesToNamedDocumentRef()
    {
        var deserialized = Deserialize<NullableDocument<NamedRef>>(NullNamedDocumentWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(NamedDocumentRefWire, serialized);
    }

    [Test]
    public void RoundTripNamedDocumentChangesToNamedDocumentReference()
    {
        var deserialized = Deserialize<NamedDocument>(NamedDocumentWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(NamedDocumentRefWire, serialized);
    }

    [Test]
    public void RoundTripNonNullNamedDocumentChangesToNamedDocumentReference()
    {
        var deserialized = Deserialize<NullableDocument<NamedDocument>>(NamedDocumentWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(NamedDocumentRefWire, serialized);
    }

    [Test]
    public void RoundTripNullNamedDocumentChangesToNamedDocumentReference()
    {
        var deserialized = Deserialize<NullableDocument<NamedDocument>>(NullNamedDocumentWire);
        var serialized = Serialize(deserialized);
        Assert.AreEqual(NamedDocumentRefWire, serialized);
    }

    [Test]
    public void RegisterDeregisterCustomSerializer()
    {
        var s = new IntToStringSerializer();
        Serializer.Register(s);

        const int i = 42;
        var ser = Serialize(i);
        Assert.AreEqual(@"""42""", ser);

        var deser = Deserialize<int>(ser);
        Assert.AreEqual(i, deser);

        Serializer.Deregister(typeof(int));
        ser = Serialize(i);

        Assert.AreEqual(@"{""@int"":""42""}", ser);
        deser = Deserialize<int>(ser);
        Assert.AreEqual(i, deser);
    }
}
