using System.Text;
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Types;

namespace Fauna.Test.Serialization;

public class Helpers
{
    public const string StringWire = @"""hello""";
    public const string StringVal = "hello";

    public const string IntWire = @"{""@int"":""42""}";
    public const byte ByteVal = 42;
    public const sbyte SByteVal = 42;

    public static readonly byte[] BytesVal = { 70, 97, 117, 110, 97 }; // Encoding.UTF8.GetBytes("Fauna")
    public const string BytesWire = @"{""@bytes"":""RmF1bmE=""}";

    public const string ShortMaxWire = @"{""@int"":""32767""}";
    public const string ShortMinWire = @"{""@int"":""-32768""}";
    public const string UShortMaxWire = @"{""@int"":""65535""}";
    public const string UShortMinWire = @"{""@int"":""0""}";
    public const string IntMaxWire = @"{""@int"":""2147483647""}";
    public const string IntMinWire = @"{""@int"":""-2147483648""}";
    public const string UIntMaxWire = @"{""@long"":""4294967295""}";
    public const string UIntMinWire = @"{""@long"":""0""}";
    public const string LongMaxWire = @"{""@long"":""9223372036854775807""}";
    public const string LongMinWire = @"{""@long"":""-9223372036854775808""}";
    public const string FloatMaxWire = @"{""@double"":""3.4028234663852886E\u002B38""}";
    public const string FloatMinWire = @"{""@double"":""-3.4028234663852886E\u002B38""}";
    public const string DoubleMaxWire = @"{""@double"":""1.7976931348623157E\u002B308""}";
    public const string DoubleMinWire = @"{""@double"":""-1.7976931348623157E\u002B308""}";
    public const string DoubleNaNWire = @"{""@double"":""NaN""}";
    public const string DoubleInfWire = @"{""@double"":""Infinity""}";
    public const string DoubleNegInfWire = @"{""@double"":""-Infinity""}";
    public const string NullWire = "null";
    public const string TrueWire = "true";
    public const string FalseWire = "false";
    public const string StreamWire = @"{""@stream"": ""token""}";

    public const string DateWire = @"{""@date"":""2023-12-15""}";
    public static DateOnly DateOnlyVal => DateOnly.Parse("2023-12-15");

    public const string TimeWire = @"{""@time"":""2023-12-15T01:01:01.0010011Z""}";
    public static DateTime DateTimeVal => DateTime.Parse("2023-12-15T01:01:01.0010011Z");

    public const string TimeFromOffsetWire = @"{""@time"":""2023-12-15T01:01:01.0010011\u002B00:00""}";
    public static DateTimeOffset DateTimeOffsetVal => DateTimeOffset.Parse("2023-12-15T01:01:01.0010011Z");

    public enum TestType
    {
        Serialize,
        Deserialize,
        Roundtrip,
    }

    public record struct SerializeTest<T>
    {
        public ISerializer<T> Serializer { get; set; }
        public object? Value { get; set; }
        public object? Expected { get; set; }
        public TestType TestType { get; set; }

        public string? Throws { get; set; }
    }
    public static EventSource EventSourceVal => new("token");

    public static string Serialize(ISerializer s, MappingContext ctx, object? obj)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8FaunaWriter(stream);

        s.Serialize(ctx, writer, obj);

        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static object? Deserialize(ISerializer s, MappingContext ctx, string str)
    {
        var reader = new Utf8FaunaReader(str);
        reader.Read();
        var obj = s.Deserialize(ctx, ref reader);
        if (reader.Read())
        {
            throw new SerializationException($"Token stream is not exhausted but should be: {reader.CurrentTokenType}");
        }

        return obj;
    }

    public static T? Deserialize<T>(ISerializer<T> s, MappingContext ctx, string str)
    {
        var reader = new Utf8FaunaReader(str);
        reader.Read();
        var obj = s.Deserialize(ctx, ref reader);
        if (reader.Read())
        {
            throw new SerializationException($"Token stream is not exhausted but should be: {reader.CurrentTokenType}");
        }

        return (T?)obj;
    }
}
