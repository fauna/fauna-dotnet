using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Serialization.Serializers;
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
    [TestCase("Fauna.Test.Serialization.ClassWithDupeFields")]
    [TestCase("Fauna.Test.Serialization.ClassWithFieldNameOverlap")]
    public void InvalidFieldNamesThrowsArgumentException(string classWithBadFields)
    {
        var ex = Assert.Throws<ArgumentException>(() =>
        {
            var badClass = Type.GetType(classWithBadFields);
            Assert.IsNotNull(badClass, $"Couldn't find Type from class name: {classWithBadFields}");
            var serializer = Serializer.Generate(_ctx, badClass!);
        });

        Assert.IsNotNull(ex);
        Assert.IsTrue(ex!.Message.Contains("Duplicate field name"));
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
            "dateTimeField",
            "dateOnlyField",
            "dateTimeOffsetField",
            "stringField",
            "shortField",
            "ushortField",
            "intField",
            "uintField",
            "floatField",
            "doubleField",
            "longField",
            "boolField",
            "byteField",
            "sbyteField",
            "nullableIntField",
            "otherDocRef"
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
                case "dateTimeField":
                    Assert.IsFalse(field.IsNullable);
                    Assert.AreEqual(typeof(DateTime), field.Type);
                    Assert.IsInstanceOf<DateTimeSerializer>(field.Serializer);
                    break;
                case "dateOnlyField":
                    Assert.IsFalse(field.IsNullable);
                    Assert.AreEqual(typeof(DateOnly), field.Type);
                    Assert.IsInstanceOf<DateOnlySerializer>(field.Serializer);
                    break;
                case "dateTimeOffsetField":
                    Assert.IsFalse(field.IsNullable);
                    Assert.AreEqual(typeof(DateTimeOffset), field.Type);
                    Assert.IsInstanceOf<DateTimeOffsetSerializer>(field.Serializer);
                    break;
                case "stringField":
                    Assert.IsTrue(field.IsNullable);
                    Assert.AreEqual(typeof(string), field.Type);
                    Assert.IsInstanceOf<NullableSerializer<string>>(field.Serializer);
                    break;
                case "shortField":
                    Assert.IsFalse(field.IsNullable);
                    Assert.AreEqual(typeof(short), field.Type);
                    Assert.IsInstanceOf<ShortSerializer>(field.Serializer);
                    break;
                case "ushortField":
                    Assert.IsFalse(field.IsNullable);
                    Assert.AreEqual(typeof(ushort), field.Type);
                    Assert.IsInstanceOf<UShortSerializer>(field.Serializer);
                    break;
                case "intField":
                    Assert.IsFalse(field.IsNullable);
                    Assert.AreEqual(typeof(int), field.Type);
                    Assert.IsInstanceOf<IntSerializer>(field.Serializer);
                    break;
                case "uintField":
                    Assert.IsFalse(field.IsNullable);
                    Assert.AreEqual(typeof(uint), field.Type);
                    Assert.IsInstanceOf<UIntSerializer>(field.Serializer);
                    break;
                case "floatField":
                    Assert.IsFalse(field.IsNullable);
                    Assert.AreEqual(typeof(float), field.Type);
                    Assert.IsInstanceOf<FloatSerializer>(field.Serializer);
                    break;
                case "doubleField":
                    Assert.IsFalse(field.IsNullable);
                    Assert.AreEqual(typeof(double), field.Type);
                    Assert.IsInstanceOf<DoubleSerializer>(field.Serializer);
                    break;
                case "longField":
                    Assert.IsFalse(field.IsNullable);
                    Assert.AreEqual(typeof(long), field.Type);
                    Assert.IsInstanceOf<LongSerializer>(field.Serializer);
                    break;
                case "boolField":
                    Assert.IsFalse(field.IsNullable);
                    Assert.AreEqual(typeof(bool), field.Type);
                    Assert.IsInstanceOf<BooleanSerializer>(field.Serializer);
                    break;
                case "byteField":
                    Assert.IsFalse(field.IsNullable);
                    Assert.AreEqual(typeof(byte), field.Type);
                    Assert.IsInstanceOf<ByteSerializer>(field.Serializer);
                    break;
                case "sbyteField":
                    Assert.IsFalse(field.IsNullable);
                    Assert.AreEqual(typeof(sbyte), field.Type);
                    Assert.IsInstanceOf<SByteSerializer>(field.Serializer);
                    break;
                case "nullableIntField":
                    Assert.IsTrue(field.IsNullable);
                    Assert.AreEqual(typeof(int?), field.Type);
                    Assert.IsInstanceOf<NullableStructSerializer<int>>(field.Serializer);
                    break;
                case "otherDocRef":
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
