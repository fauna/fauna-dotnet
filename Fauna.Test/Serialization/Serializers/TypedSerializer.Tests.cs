using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Serialization;
using NUnit.Framework;
using Stream = Fauna.Types.Stream;

namespace Fauna.Test.Serialization;

[TestFixture]
public class TypedSerializerTests
{
    private static readonly MappingContext s_ctx = new();
    private static readonly StringSerializer _string = new();
    private static readonly ByteSerializer _byte = new();
    private static readonly SByteSerializer _sbyte = new();
    private static readonly ShortSerializer _short = new();
    private static readonly UShortSerializer _ushort = new();
    private static readonly IntSerializer _int = new();
    private static readonly UIntSerializer _uint = new();
    private static readonly LongSerializer _long = new();
    private static readonly FloatSerializer _float = new();
    private static readonly DoubleSerializer _double = new();
    private static readonly BooleanSerializer _bool = new();
    private static readonly DateOnlySerializer _date = new();
    private static readonly DateTimeSerializer _time = new();
    private static readonly DateTimeOffsetSerializer _offset = new();
    private static readonly StreamSerializer _stream = new();

    [Test]
    [TestCaseSource(nameof(TypedSerializerCases))]
    public void TestTypedSerializers<T>(Helpers.SerializeTest<T> t)
    {
        switch (t.TestType)
        {
            case Helpers.TestType.Serialize:
                if (t.Throws is null)
                {
                    string rs = Helpers.Serialize(t.Serializer, s_ctx, t.Value);
                    Assert.AreEqual(t.Expected, rs);
                }
                else
                {
                    var ex = Assert.Throws<SerializationException>(() => Helpers.Serialize(t.Serializer, s_ctx, t.Value));
                    Assert.That(ex!.Message, Is.EqualTo(t.Throws));
                }
                break;

            case Helpers.TestType.Deserialize:
                if (t.Throws is null)
                {
                    var rd = Helpers.Deserialize(t.Serializer, s_ctx, (string)t.Value!);
                    Assert.AreEqual(t.Expected, rd);
                }
                else
                {
                    var ex = Assert.Throws<SerializationException>(() => Helpers.Deserialize(t.Serializer, s_ctx, (string)t.Value!));
                    Assert.That(ex!.Message, Is.EqualTo(t.Throws));
                }
                break;

            case Helpers.TestType.Roundtrip:
                string serialized = Helpers.Serialize(t.Serializer, s_ctx, t.Value);
                Assert.AreEqual(t.Expected, serialized);

                var deserialized = Helpers.Deserialize(t.Serializer, s_ctx, serialized);
                Assert.AreEqual(t.Value, deserialized);
                break;

            default:
                Assert.Fail("Unhandled TestType");
                break;
        }
    }

    public static object[] TypedSerializerCases =
    {
        // StringSerializer
        new object[]
        {
            new Helpers.SerializeTest<string?>
            {
                Serializer = _string,
                Value = Helpers.StringVal,
                Expected = Helpers.StringWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<string?>
            {
                Serializer = _string,
                Value = null,
                Expected = Helpers.NullWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<string?>
            {
                Serializer = _string,
                Value = Helpers.IntMaxWire,
                Throws = "Unexpected token `Int` deserializing with `StringSerializer`",
                TestType = Helpers.TestType.Deserialize
            }
        },

        // ByteSerializer
        new object[]
        {
            new Helpers.SerializeTest<byte>
            {
                Serializer = _byte,
                Value = Helpers.ByteVal,
                Expected = Helpers.IntWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<byte>
            {
                Serializer = _byte,
                Value = null,
                Expected = Helpers.NullWire,
                TestType = Helpers.TestType.Serialize
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<byte>
            {
                Serializer = _byte,
                Value = Helpers.LongMaxWire,
                Throws = "Unexpected token `Long` deserializing with `ByteSerializer`",
                TestType = Helpers.TestType.Deserialize
            }
        },

        // SByteSerializer
        new object[]
        {
            new Helpers.SerializeTest<sbyte>
            {
                Serializer = _sbyte,
                Value = Helpers.SByteVal,
                Expected = Helpers.IntWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<sbyte>
            {
                Serializer = _sbyte,
                Value = null,
                Expected = Helpers.NullWire,
                TestType = Helpers.TestType.Serialize
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<sbyte>
            {
                Serializer = _sbyte,
                Value = Helpers.LongMaxWire,
                Throws = "Unexpected token `Long` deserializing with `SByteSerializer`",
                TestType = Helpers.TestType.Deserialize
            }
        },

        // ShortSerializer
        new object[]
        {
            new Helpers.SerializeTest<short>
            {
                Serializer = _short,
                Value = short.MaxValue,
                Expected = Helpers.ShortMaxWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<short>
            {
                Serializer = _short,
                Value = short.MinValue,
                Expected = Helpers.ShortMinWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<short>
            {
                Serializer = _short,
                Value = null,
                Expected = Helpers.NullWire,
                TestType = Helpers.TestType.Serialize
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<short>
            {
                Serializer = _short,
                Value = Helpers.LongMaxWire,
                Throws = "Unexpected token `Long` deserializing with `ShortSerializer`",
                TestType = Helpers.TestType.Deserialize
            }
        },

        // UShortSerializer
        new object[]
        {
            new Helpers.SerializeTest<ushort>
            {
                Serializer = _ushort,
                Value = ushort.MaxValue,
                Expected = Helpers.UShortMaxWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<ushort>
            {
                Serializer = _ushort,
                Value = ushort.MinValue,
                Expected = Helpers.UShortMinWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<ushort>
            {
                Serializer = _ushort,
                Value = null,
                Expected = Helpers.NullWire,
                TestType = Helpers.TestType.Serialize
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<ushort>
            {
                Serializer = _ushort,
                Value = Helpers.LongMaxWire,
                Throws = "Unexpected token `Long` deserializing with `UShortSerializer`",
                TestType = Helpers.TestType.Deserialize
            }
        },

        // IntSerializer
        new object[]
        {
            new Helpers.SerializeTest<int>
            {
                Serializer = _int,
                Value = int.MaxValue,
                Expected = Helpers.IntMaxWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<int>
            {
                Serializer = _int,
                Value = int.MinValue,
                Expected = Helpers.IntMinWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<int>
            {
                Serializer = _int,
                Value = null,
                Expected = Helpers.NullWire,
                TestType = Helpers.TestType.Serialize
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<int>
            {
                Serializer = _int,
                Value = Helpers.LongMaxWire,
                Throws = "Unexpected token `Long` deserializing with `IntSerializer`",
                TestType = Helpers.TestType.Deserialize
            }
        },

        // UIntSerializer
        new object[]
        {
            new Helpers.SerializeTest<uint>
            {
                Serializer = _uint,
                Value = uint.MaxValue,
                Expected = Helpers.UIntMaxWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<uint>
            {
                Serializer = _uint,
                Value = uint.MinValue,
                Expected = Helpers.UIntMinWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<uint>
            {
                Serializer = _uint,
                Value = null,
                Expected = Helpers.NullWire,
                TestType = Helpers.TestType.Serialize
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<uint>
            {
                Serializer = _uint,
                Value = Helpers.NullWire,
                Throws = "Unexpected token `Null` deserializing with `UIntSerializer`",
                TestType = Helpers.TestType.Deserialize
            }
        },

        // LongSerializer
        new object[]
        {
            new Helpers.SerializeTest<long>
            {
                Serializer = _long,
                Value = long.MaxValue,
                Expected = Helpers.LongMaxWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<long>
            {
                Serializer = _long,
                Value = long.MinValue,
                Expected = Helpers.LongMinWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<long>
            {
                Serializer = _long,
                Value = null,
                Expected = Helpers.NullWire,
                TestType = Helpers.TestType.Serialize
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<long>
            {
                Serializer = _long,
                Value = Helpers.NullWire,
                Throws = "Unexpected token `Null` deserializing with `LongSerializer`",
                TestType = Helpers.TestType.Deserialize
            }
        },

        // FloatSerializer
        new object[]
        {
            new Helpers.SerializeTest<float>
            {
                Serializer = _float,
                Value = float.MaxValue,
                Expected = Helpers.FloatMaxWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<float>
            {
                Serializer = _float,
                Value = float.MinValue,
                Expected = Helpers.FloatMinWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<float>
            {
                Serializer = _float,
                Value = null,
                Expected = Helpers.NullWire,
                TestType = Helpers.TestType.Serialize
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<float>
            {
                Serializer = _float,
                Value = Helpers.NullWire,
                Throws = "Unexpected token `Null` deserializing with `FloatSerializer`",
                TestType = Helpers.TestType.Deserialize
            }
        },

        // DoubleSerializer
        new object[]
        {
            new Helpers.SerializeTest<double>
            {
                Serializer = _double,
                Value = double.MaxValue,
                Expected = Helpers.DoubleMaxWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<double>
            {
                Serializer = _double,
                Value = double.MinValue,
                Expected = Helpers.DoubleMinWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<double>
            {
                Serializer = _double,
                Value = double.NaN,
                Expected = Helpers.DoubleNaNWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<double>
            {
                Serializer = _double,
                Value = double.NegativeInfinity,
                Expected = Helpers.DoubleNegInfWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<double>
            {
                Serializer = _double,
                Value = double.PositiveInfinity,
                Expected = Helpers.DoubleInfWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<double>
            {
                Serializer = _double,
                Value = null,
                Expected = Helpers.NullWire,
                TestType = Helpers.TestType.Serialize
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<double>
            {
                Serializer = _double,
                Value = Helpers.NullWire,
                Throws = "Unexpected token `Null` deserializing with `DoubleSerializer`",
                TestType = Helpers.TestType.Deserialize
            }
        },

        // BooleanSerializer
        new object[]
        {
            new Helpers.SerializeTest<bool>
            {
                Serializer = _bool,
                Value = true,
                Expected = Helpers.TrueWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<bool>
            {
                Serializer = _bool,
                Value = false,
                Expected = Helpers.FalseWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<bool>
            {
                Serializer = _bool,
                Value = null,
                Expected = Helpers.NullWire,
                TestType = Helpers.TestType.Serialize
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<bool>
            {
                Serializer = _bool,
                Value = Helpers.NullWire,
                Throws = "Unexpected token `Null` deserializing with `BooleanSerializer`",
                TestType = Helpers.TestType.Deserialize
            }
        },

        // DateOnlySerializer
        new object[]
        {
            new Helpers.SerializeTest<DateOnly>
            {
                Serializer = _date,
                Value = Helpers.DateOnlyVal,
                Expected = Helpers.DateWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<DateOnly>
            {
                Serializer = _date,
                Value = null,
                Expected = Helpers.NullWire,
                TestType = Helpers.TestType.Serialize
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<DateOnly>
            {
                Serializer = _date,
                Value = Helpers.NullWire,
                Throws = "Unexpected token `Null` deserializing with `DateOnlySerializer`",
                TestType = Helpers.TestType.Deserialize
            }
        },

        // DateTimeSerializer
        new object[]
        {
            new Helpers.SerializeTest<DateTime>
            {
                Serializer = _time,
                Value = Helpers.DateTimeVal,
                Expected = Helpers.TimeWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<DateTime>
            {
                Serializer = _time,
                Value = null,
                Expected = Helpers.NullWire,
                TestType = Helpers.TestType.Serialize
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<DateTime>
            {
                Serializer = _time,
                Value = Helpers.NullWire,
                Throws = "Unexpected token `Null` deserializing with `DateTimeSerializer`",
                TestType = Helpers.TestType.Deserialize
            }
        },

        // DateTimeOffsetSerializer
        new object[]
        {
            new Helpers.SerializeTest<DateTimeOffset>
            {
                Serializer = _offset,
                Value = Helpers.DateTimeOffsetVal,
                Expected = Helpers.TimeFromOffsetWire,
                TestType = Helpers.TestType.Roundtrip
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<DateTimeOffset>
            {
                Serializer = _offset,
                Value = null,
                Expected = Helpers.NullWire,
                TestType = Helpers.TestType.Serialize
            }
        },
        new object[]
        {
            new Helpers.SerializeTest<DateTimeOffset>
            {
                Serializer = _offset,
                Value = Helpers.NullWire,
                Throws = "Unexpected token `Null` deserializing with `DateTimeOffsetSerializer`",
                TestType = Helpers.TestType.Deserialize
            }
        },

        // StreamSerializer
        new object[]
        {
            new Helpers.SerializeTest<Stream>
            {
                Serializer = _stream,
                Value = Helpers.StreamWire,
                Expected = Helpers.StreamVal,
                TestType = Helpers.TestType.Deserialize
            }
        }
    };
}
