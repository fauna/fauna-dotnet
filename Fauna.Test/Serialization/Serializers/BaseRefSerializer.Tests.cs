
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

public class BaseRefSerializerTests
{
    private readonly MappingContext _ctx;
    private const string DocumentWire = @"{""@doc"":{""id"":""123"",""coll"":{""@mod"":""MyColl""},""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},""user_field"":""user_value""}}";
    private const string MappedDocumentWire = @"{""@doc"":{""id"":""123"",""coll"":{""@mod"":""MappedColl""},""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},""user_field"":""user_value""}}";
    private const string DocumentRefWire = @"{""@ref"":{""id"":""123"",""coll"":{""@mod"":""MyColl""}}}";
    private const string NullDocumentWire = @"{""@ref"":{""id"":""123"",""coll"":{""@mod"":""MyColl""},""exists"":false,""cause"":""not found""}}";
    private const string NamedDocumentWire = @"{""@doc"":{""name"":""Foo"",""coll"":{""@mod"":""MyColl""},""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},""user_field"":""user_value""}}";
    private const string NullNamedDocumentWire = @"{""@ref"":{""name"":""Foo"",""coll"":{""@mod"":""MyColl""},""exists"":false,""cause"":""not found""}}";
    private const string NamedDocumentRefWire = @"{""@ref"":{""name"":""Foo"",""coll"":{""@mod"":""MyColl""}}}";

    public BaseRefSerializerTests()
    {
        var collections = new Dictionary<string, Type> {
            { "MappedColl", typeof(ClassForDocument) }
        };
        _ctx = new MappingContext(collections);
    }

    [Test]
    public void RoundTripRef()
    {
        var serializer = Serializer.Generate<Ref<object>>(_ctx);
        var deserialized = Helpers.Deserialize(serializer, _ctx, DocumentRefWire)!;
        Assert.AreEqual("123", deserialized.Id);
        Assert.AreEqual("MyColl", deserialized.Collection.Name);

        string serialized = Helpers.Serialize(serializer, _ctx, deserialized);
        Assert.AreEqual(DocumentRefWire, serialized);
    }


    [Test]
    public void RoundTripDocument()
    {
        var serializer = Serializer.Generate<Ref<ClassForDocument>>(_ctx);
        var deserialized = Helpers.Deserialize(serializer, _ctx, DocumentWire)!;
        Assert.AreEqual("123", deserialized.Id);
        Assert.AreEqual("MyColl", deserialized.Collection.Name);

        var doc = deserialized.Get();
        Assert.AreEqual("123", doc.Id);
        Assert.AreEqual("MyColl", doc.Coll?.Name);
        Assert.AreEqual(DateTime.Parse("2023-12-15T01:01:01.0010010Z"), doc.Ts);
        Assert.AreEqual("user_value", doc.UserField);

        string serialized = Helpers.Serialize(serializer, _ctx, deserialized);
        Assert.AreEqual(DocumentRefWire, serialized);
    }

    [Test]
    public void RoundTripNullDoc()
    {
        var serializer = Serializer.Generate<Ref<ClassForDocument>>(_ctx);
        var deserialized = Helpers.Deserialize(serializer, _ctx, NullDocumentWire)!;
        Assert.AreEqual("123", deserialized.Id);
        Assert.AreEqual("MyColl", deserialized.Collection.Name);
        Assert.IsFalse(deserialized.Exists);
        Assert.AreEqual("not found", deserialized.Cause);

        var ex = Assert.Throws<NullDocumentException>(() => deserialized.Get())!;
        Assert.AreEqual("123", ex.Id);
        Assert.AreEqual("MyColl", ex.Collection.Name);
        Assert.AreEqual("not found", ex.Cause);

        string serialized = Helpers.Serialize(serializer, _ctx, deserialized);
        Assert.AreEqual(DocumentRefWire, serialized);
    }

    [Test]
    public void RoundTripNamedRef()
    {
        var serializer = Serializer.Generate<NamedRef<object>>(_ctx);
        var deserialized = Helpers.Deserialize(serializer, _ctx, NamedDocumentRefWire)!;
        string serialized = Helpers.Serialize(serializer, _ctx, deserialized);
        Assert.AreEqual(NamedDocumentRefWire, serialized);
    }

    [Test]
    public void RoundTripNamedDocument()
    {
        var serializer = Serializer.Generate<NamedRef<Dictionary<string, object>>>(_ctx);
        var deserialized = Helpers.Deserialize(serializer, _ctx, NamedDocumentWire)!;
        Assert.AreEqual("Foo", deserialized.Name);
        Assert.AreEqual("MyColl", deserialized.Collection.Name);

        var doc = deserialized.Get();
        Assert.AreEqual(DateTime.Parse("2023-12-15T01:01:01.0010010Z"), doc["ts"]);
        Assert.AreEqual("user_value", doc["user_field"]);

        string serialized = Helpers.Serialize(serializer, _ctx, deserialized);
        Assert.AreEqual(NamedDocumentRefWire, serialized);
    }

    [Test]
    public void RoundTripNamedDocumentAsClass()
    {
        var serializer = Serializer.Generate<NamedRef<ClassForNamedDocument>>(_ctx);
        var deserialized = Helpers.Deserialize(serializer, _ctx, NamedDocumentWire)!;
        Assert.AreEqual("Foo", deserialized.Name);
        Assert.AreEqual("MyColl", deserialized.Collection.Name);

        var doc = deserialized.Get();
        Assert.AreEqual("Foo", doc.Name);
        Assert.AreEqual("user_value", doc.UserField);

        string serialized = Helpers.Serialize(serializer, _ctx, deserialized);
        Assert.AreEqual(NamedDocumentRefWire, serialized);
    }

    [Test]
    public void RoundTripNullNamedDoc()
    {
        var serializer = Serializer.Generate<NamedRef<object>>(_ctx);
        var deserialized = Helpers.Deserialize(serializer, _ctx, NullNamedDocumentWire)!;
        Assert.AreEqual("Foo", deserialized.Name);
        Assert.AreEqual("MyColl", deserialized.Collection.Name);
        Assert.IsFalse(deserialized.Exists);
        Assert.AreEqual("not found", deserialized.Cause);

        string serialized = Helpers.Serialize(serializer, _ctx, deserialized);
        Assert.AreEqual(NamedDocumentRefWire, serialized);
    }

    [Test]
    public void DeserializeRegisteredClassToDictionary()
    {
        var serializer = Serializer.Generate<Ref<Dictionary<string, object>>>(_ctx);

        var actual = Helpers.Deserialize(serializer, _ctx, MappedDocumentWire)!;
        Assert.AreEqual("123", actual.Id);
        Assert.AreEqual(new Module("MappedColl"), actual.Collection);

        var doc = actual.Get();
        Assert.AreEqual(DateTime.Parse("2023-12-15T01:01:01.0010010Z"), doc["ts"]);
        Assert.AreEqual("user_value", doc["user_field"]);
    }

    [Test]
    public void DeserializeDocumentToDictionary()
    {
        var serializer = Serializer.Generate<Ref<Dictionary<string, object>>>(_ctx);
        var actual = Helpers.Deserialize(serializer, _ctx, DocumentWire)!;

        Assert.AreEqual("123", actual.Id);
        Assert.AreEqual(new Module("MyColl"), actual.Collection);

        var dict = actual.Get();
        Assert.AreEqual("123", dict["id"]);
        Assert.AreEqual(new Module("MyColl"), dict["coll"]);
        Assert.AreEqual(DateTime.Parse("2023-12-15T01:01:01.0010010Z"), dict["ts"]);
        Assert.AreEqual("user_value", dict["user_field"]);
    }
}
