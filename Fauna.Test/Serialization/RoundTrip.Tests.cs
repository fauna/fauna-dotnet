using Fauna.Mapping;
using Fauna.Serialization;
using NUnit.Framework;
using System.Text;

namespace Fauna.Test.Serialization;

[TestFixture]
public class RoundTripTests
{
    private static readonly MappingContext ctx = new();

    public static string Serialize(object? obj)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8FaunaWriter(stream);
        Serializer.Serialize(ctx, writer, obj);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static T Deserialize<T>(string str) where T : notnull
    {
        var reader = new Utf8FaunaReader(str);
        reader.Read();
        var obj = Deserializer.Generate<T>(ctx).Deserialize(ctx, ref reader);
        if (reader.Read())
        {
            throw new SerializationException($"Token stream is not exhausted but should be: {reader.CurrentTokenType}");
        }

        return obj;
    }

    [Test]
    public void RoundTripByte()
    {
        const byte test = 3;
        var serialized = Serialize(test);
        var deserialized = Deserialize<byte>(serialized);
        Assert.AreEqual(test, deserialized);
    }

    [Test]
    public void RoundTripSByte()
    {
        const sbyte test = 3;
        var serialized = Serialize(test);
        var deserialized = Deserialize<sbyte>(serialized);
        Assert.AreEqual(test, deserialized);
    }

    [Test]
    public void RoundTripShort()
    {
        const short test = 40;
        var serialized = Serialize(test);
        var deserialized = Deserialize<short>(serialized);
        Assert.AreEqual(test, deserialized);
    }

    [Test]
    public void RoundTripUShort()
    {
        const ushort test = 40;
        var serialized = Serialize(test);
        var deserialized = Deserialize<ushort>(serialized);
        Assert.AreEqual(test, deserialized);
    }

    [Test]
    public void RoundTripInt()
    {
        const int test = 40;
        var serialized = Serialize(test);
        var deserialized = Deserialize<short>(serialized);
        Assert.AreEqual(test, deserialized);
    }

    [Test]
    public void RoundTripUInt()
    {
        const uint test = 40;
        var serialized = Serialize(test);
        var deserialized = Deserialize<uint>(serialized);
        Assert.AreEqual(test, deserialized);
    }

    [Test]
    public void RoundTripLong()
    {
        const long test = 40;
        var serialized = Serialize(test);
        var deserialized = Deserialize<long>(serialized);
        Assert.AreEqual(test, deserialized);
    }

    [Test]
    public void RoundTripFloat()
    {
        const float test = 40.2f;
        var serialized = Serialize(test);
        var deserialized = Deserialize<float>(serialized);
        Assert.AreEqual(test, deserialized);
    }

    [Test]
    public void RoundTripDouble()
    {
        const double test = 40.2d;
        var serialized = Serialize(test);
        var deserialized = Deserialize<double>(serialized);
        Assert.AreEqual(test, deserialized);
    }
}
