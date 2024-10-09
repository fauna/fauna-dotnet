using System.Text;
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Serialization;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

public class SeralizerTests
{
    private static readonly MappingContext s_ctx = new();

    public static string Serialize(object? obj)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8FaunaWriter(stream);

        ISerializer ser = DynamicSerializer.Singleton;
        if (obj is not null) ser = Serializer.Generate(s_ctx, obj.GetType());
        ser.Serialize(s_ctx, writer, obj);

        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static T Deserialize<T>(string str) where T : notnull
    {
        var reader = new Utf8FaunaReader(str);
        reader.Read();
        var obj = Serializer.Generate<T>(s_ctx).Deserialize(s_ctx, ref reader);
        if (reader.Read())
        {
            throw new SerializationException($"Token stream is not exhausted but should be: {reader.CurrentTokenType}");
        }

        return obj;
    }


    [Test]
    public void RegisterDeregisterCustomSerializer()
    {
        var s = new IntToStringSerializer();
        Serializer.Register(s);

        const int i = 42;
        string ser = Serialize(i);
        Assert.AreEqual(@"""42""", ser);

        int deser = Deserialize<int>(ser);
        Assert.AreEqual(i, deser);

        Serializer.Deregister(typeof(int));
        ser = Serialize(i);

        Assert.AreEqual(@"{""@int"":""42""}", ser);
        deser = Deserialize<int>(ser);
        Assert.AreEqual(i, deser);
    }
}
