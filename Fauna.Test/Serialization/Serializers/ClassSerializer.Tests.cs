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

    [Test]
    public void DuplicateFieldNamesThrowsArgumentException()
    {
        try
        {
            var serializer = Serializer.Generate<ClassWithDupeFields>(_ctx);
        }
        catch (Exception e)
        {
            Assert.AreEqual(e.GetType(), typeof(ArgumentException));
            Assert.IsTrue(e.Message.Contains("Duplicate field name"));
            return;
        }

        Assert.Fail("Deserialization succeeded unexpectedly.");
    }

    [Test]
    public void ValidateFieldInfoOnSerializerCtx()
    {
        var mappingInfo = _ctx.GetInfo(typeof(ClassWithLotsOfFields));
        Assert.IsNotNull(mappingInfo);
        Assert.AreEqual(typeof(ClassSerializer<ClassWithLotsOfFields>), mappingInfo.ClassSerializer.GetType());
        Assert.AreEqual(typeof(ClassWithLotsOfFields), mappingInfo.Type);

        var fields = mappingInfo.FieldsByName;
        var fieldNames = new List<string>
        {
            "id",
            "coll",
            "ts",
            "string_field",
            "int_field",
            "float_field",
            "double_field",
            "bool_field",
            "nullableIntField",
            "other_doc"
        };

        Assert.IsNotNull(fields);
        Assert.IsNotEmpty(fields);
        Assert.AreEqual(fieldNames.OrderBy(x => x), fields.Keys.ToList().OrderBy(x => x));

        foreach (var name in fieldNames)
        {
            Assert.IsTrue(fields.ContainsKey(name));

            var field = fields[name];

            switch (name)
            {
                case "id":
                    Assert.IsTrue(field.IsNullable);
                    Assert.AreEqual(typeof(string), field.Type);
                    Assert.IsInstanceOf<NullableSerializer<string>>(field.Serializer);
                    break;
                case "coll":
                    Assert.IsTrue(field.IsNullable);
                    Assert.AreEqual(typeof(Module), field.Type);
                    Assert.IsInstanceOf<NullableSerializer<Module>>(field.Serializer);
                    break;
                case "ts":
                    Assert.IsTrue(field.IsNullable);
                    Assert.AreEqual(typeof(DateTime?), field.Type);
                    Assert.IsInstanceOf<NullableStructSerializer<DateTime>>(field.Serializer);
                    break;
                case "string_field":
                    Assert.IsTrue(field.IsNullable);
                    Assert.AreEqual(typeof(string), field.Type);
                    Assert.IsInstanceOf<NullableSerializer<string>>(field.Serializer);
                    break;
                case "int_field":
                    Assert.IsFalse(field.IsNullable);
                    Assert.AreEqual(typeof(int), field.Type);
                    Assert.IsInstanceOf<IntSerializer>(field.Serializer);
                    break;
                case "float_field":
                    Assert.IsFalse(field.IsNullable);
                    Assert.AreEqual(typeof(float), field.Type);
                    Assert.IsInstanceOf<FloatSerializer>(field.Serializer);
                    break;
                case "double_field":
                    Assert.IsFalse(field.IsNullable);
                    Assert.AreEqual(typeof(double), field.Type);
                    Assert.IsInstanceOf<DoubleSerializer>(field.Serializer);
                    break;
                case "bool_field":
                    Assert.IsFalse(field.IsNullable);
                    Assert.AreEqual(typeof(bool), field.Type);
                    Assert.IsInstanceOf<BooleanSerializer>(field.Serializer);
                    break;
                case "nullableIntField":
                    Assert.IsTrue(field.IsNullable);
                    Assert.AreEqual(typeof(int?), field.Type);
                    Assert.IsInstanceOf<NullableStructSerializer<int>>(field.Serializer);
                    break;
                case "other_doc":
                    Assert.IsTrue(field.IsNullable);
                    Assert.AreEqual(typeof(ClassForDocument), field.Type);
                    Assert.IsInstanceOf<NullableSerializer<ClassForDocument>>(field.Serializer);
                    break;
                default:
                    Assert.Fail($"Unhandled field name: {field.Name}");
                    break;
            }
        }
    }
}
