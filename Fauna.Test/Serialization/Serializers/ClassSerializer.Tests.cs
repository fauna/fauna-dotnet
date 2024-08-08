using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

public class ClassSerializerTests
{
    private readonly MappingContext _ctx;

    public ClassSerializerTests()
    {
        var colls = new Dictionary<string, Type> {
            { "MappedColl", typeof(ClassForDocument) }
        };
        _ctx = new MappingContext(colls);
    }

    [Test]
    public void RoundTripListClassWithoutAttributes()
    {
        var serializer = Serializer.Generate<Person>(_ctx);

        const string wire = @"{""firstName"":""Alice"",""lastName"":""Smith"",""age"":{""@int"":""100""}}";
        var alice = new Person { FirstName = "Alice", LastName = "Smith", Age = 100 };

        var deserialized = Helpers.Deserialize(serializer, _ctx, wire);
        Assert.AreEqual(alice, deserialized);

        string serialized = Helpers.Serialize(serializer, _ctx, deserialized);
        Assert.AreEqual(wire, serialized);
    }

    [Test]
    public void RoundTripListClassWithAttributes()
    {
        var serializer = Serializer.Generate<PersonWithAttributes>(_ctx);

        const string wire = @"{""first_name"":""Alice"",""last_name"":""Smith"",""age"":{""@int"":""100""}}";
        var alice = new PersonWithAttributes { FirstName = "Alice", LastName = "Smith", Age = 100 };

        var deserialized = Helpers.Deserialize(serializer, _ctx, wire);
        Assert.AreEqual(alice, deserialized);

        string serialized = Helpers.Serialize(serializer, _ctx, deserialized);
        Assert.AreEqual(wire, serialized);
    }

    [Test]
    public void RoundTripClassWithTagConflicts()
    {
        {
            var tests = new Dictionary<IOnlyField, string>()
            {
                { new PersonWithDateConflict(), @"{""@object"":{""@date"":""not""}}" },
                { new PersonWithDocConflict(), @"{""@object"":{""@doc"":""not""}}" },
                { new PersonWithDoubleConflict(), @"{""@object"":{""@double"":""not""}}" },
                { new PersonWithIntConflict(), @"{""@object"":{""@int"":""not""}}" },
                { new PersonWithLongConflict(), @"{""@object"":{""@long"":""not""}}" },
                { new PersonWithModConflict(), @"{""@object"":{""@mod"":""not""}}" },
                { new PersonWithObjectConflict(), @"{""@object"":{""@object"":""not""}}" },
                { new PersonWithRefConflict(), @"{""@object"":{""@ref"":""not""}}" },
                { new PersonWithSetConflict(), @"{""@object"":{""@set"":""not""}}" },
                { new PersonWithTimeConflict(), @"{""@object"":{""@time"":""not""}}" }
            };

            foreach ((IOnlyField obj, string wire) in tests)
            {
                var serializer = (ISerializer<IOnlyField>)Serializer.Generate(_ctx, obj.GetType());

                var deserialized = Helpers.Deserialize(serializer, _ctx, wire);
                Assert.AreEqual(obj.Field, deserialized!.Field);

                string serialized = Helpers.Serialize(serializer, _ctx, deserialized);
                Assert.AreEqual(wire, serialized);
            }
        }
    }

    [Test]
    public void SerializeMappedClassSkipsNullIdCollTs()
    {
        var serializer = Serializer.Generate<ClassForDocument>(_ctx);
        const string wire = @"{""user_field"":""foo""}";
        var obj = new ClassForDocument
        {
            UserField = "foo"
        };

        string serialized = Helpers.Serialize(serializer, _ctx, obj);
        Assert.AreEqual(wire, serialized);
    }

    [Test]
    public void SerializeMappedClassDoesNotSkipNonNullIdCollTs()
    {
        var serializer = Serializer.Generate<ClassForDocument>(_ctx);
        const string wire = @"{""id"":""123"",""coll"":{""@mod"":""MappedColl""},""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},""user_field"":""foo""}";
        var obj = new ClassForDocument
        {
            Id = "123",
            Coll = new Module("MappedColl"),
            Ts = DateTime.Parse("2023-12-15T01:01:01.0010010Z"),
            UserField = "foo"
        };

        string serialized = Helpers.Serialize(serializer, _ctx, obj);
        Assert.AreEqual(wire, serialized);
    }

    [Test]
    public void SerializeUnmappedClassDoesNotSkipNullIdCollTs()
    {
        var serializer = Serializer.Generate<ClassForUnmapped>(_ctx);
        const string wire = @"{""id"":null,""coll"":null,""ts"":null,""user_field"":""foo""}";
        var obj = new ClassForUnmapped
        {
            UserField = "foo"
        };

        string serialized = Helpers.Serialize(serializer, _ctx, obj);
        Assert.AreEqual(wire, serialized);
    }

    [Test]
    public void DeserializeDocumentToUnmappedClass()
    {
        var serializer = Serializer.Generate<ClassWithShort>(_ctx);

        const string wire = @"{""@doc"":{""id"":""123"",""coll"":{""@mod"":""MyColl""},""ts"":{""@time"":""2023-12-15T01:01:01.0010010Z""},""a_short"":{""@int"":""42""}}}";
        var obj = new ClassWithShort { AShort = 42 };

        var deserialized = Helpers.Deserialize(serializer, _ctx, wire);
        Assert.AreEqual(obj.AShort, deserialized!.AShort);
    }
}
