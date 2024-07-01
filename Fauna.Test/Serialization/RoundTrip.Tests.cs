using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Types;
using NUnit.Framework;
using System.Text;
using System.Text.RegularExpressions;

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

}
