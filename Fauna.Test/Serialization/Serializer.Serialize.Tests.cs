using Fauna.Serialization;
using Fauna.Serialization.Attributes;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

[TestFixture]
public class SerializerSerializeTests
{

    private class Person
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int Age { get; set; }
    }

    [FaunaObject]
    private class PersonWithContext
    {
        [Field("first_name")]
        public string? FirstName { get; set; }
        [Field("last_name")]
        public string? LastName { get; set; }
        [Field("age", FaunaType.Long)]
        public int Age { get; set; }
    }

    [Test]
    public void SerializeValues()
    {
        var tests = new Dictionary<string, object?>
        {
            {"\"hello\"", "hello"},
            {"""{"@int":"42"}""", 42},
            {"""{"@long":"42"}""", 42L},
            {"""{"@double":"1.2"}""", 1.2d},
            {"true", true},
            {"false", false},
            {"null", null},
        };

        foreach (var entry in tests)
        {
            var result = Serializer.Serialize(entry.Value);
            Assert.AreEqual(entry.Key, result);
        }
    }

    [Test]
    public void SerializeUserDefinedClass()
    {
        var test = new Person
        {
            FirstName = "Baz",
            LastName = "Luhrmann",
            Age = 61
        };

        var actual = Serializer.Serialize(test);
        Assert.AreEqual("""{"FirstName":"Baz","LastName":"Luhrmann","Age":{"@int":"61"}}""", actual);
    }

    [Test]
    public void SerializeUserDefinedClassWithContext()
    {
        var test = new PersonWithContext
        {
            FirstName = "Baz",
            LastName = "Luhrmann",
            Age = 61
        };

        var actual = Serializer.Serialize(test);
        Assert.AreEqual("""{"first_name":"Baz","last_name":"Luhrmann","age":{"@long":"61"}}""", actual);
    }
}