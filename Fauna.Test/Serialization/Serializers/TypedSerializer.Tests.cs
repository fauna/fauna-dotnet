using Fauna.Exceptions;
using Fauna.Mapping;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

[TestFixture]
public class TypedSerializerTests
{
    private static readonly MappingContext _ctx = new();

    [Test]
    [TestCaseSource(typeof(SerializerFixtures), nameof(SerializerFixtures.TypedSerializerCases))]
    public void TestTypedSerializers<T>(Helpers.SerializeTest<T> t)
    {
        switch (t.TestType)
        {
            case Helpers.TestType.Serialize:
                if (t.Throws is null)
                {
                    var rs = Helpers.Serialize(t.Serializer, _ctx, t.Value);
                    Assert.AreEqual(t.Expected, rs);
                }
                else
                {
                    var ex = Assert.Throws<SerializationException>(() => Helpers.Serialize(t.Serializer, _ctx, t.Value));
                    Assert.That(ex!.Message, Is.EqualTo(t.Throws));
                }
                break;

            case Helpers.TestType.Deserialize:
                if (t.Throws is null)
                {
                    var rd = Helpers.Deserialize(t.Serializer, _ctx, (string)t.Value!);
                    Assert.AreEqual(t.Expected, rd);
                }
                else
                {
                    var ex = Assert.Throws<SerializationException>(() => Helpers.Deserialize(t.Serializer, _ctx, (string)t.Value!));
                    Assert.That(ex!.Message, Is.EqualTo(t.Throws));
                }
                break;

            case Helpers.TestType.Roundtrip:
                var serialized = Helpers.Serialize(t.Serializer, _ctx, t.Value);
                Assert.AreEqual(t.Expected, serialized);

                var deserialized = Helpers.Deserialize(t.Serializer, _ctx, serialized);
                Assert.AreEqual(t.Value, deserialized);
                break;

            default:
                Assert.Fail("Unhandled TestType");
                break;
        }
    }

}
