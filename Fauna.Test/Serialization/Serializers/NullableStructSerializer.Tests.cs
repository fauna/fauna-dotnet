using Fauna.Mapping;
using Fauna.Serialization;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

public class NullableStructSerializerTests
{
    private static readonly MappingContext s_ctx = new();

    [Test]
    public void RoundTripNullableStructAsNull()
    {
        const string wire = "null";
        var serializer = Serializer.Generate(s_ctx, typeof(int?));
        object? deserialized = Helpers.Deserialize(serializer, s_ctx, wire);
        Assert.IsNull(deserialized);

        string serialized = Helpers.Serialize(serializer, s_ctx, deserialized);
        Assert.AreEqual(wire, serialized);
    }

    [Test]
    public void RoundTripNullableStructAsValue()
    {
        const string wire = @"{""@int"":""42""}";
        int? obj = 42;
        var serializer = Serializer.Generate(s_ctx, typeof(int?));
        object? deserialized = Helpers.Deserialize(serializer, s_ctx, wire);
        Assert.AreEqual(obj, deserialized);

        string serialized = Helpers.Serialize(serializer, s_ctx, deserialized);
        Assert.AreEqual(wire, serialized);
    }
}
