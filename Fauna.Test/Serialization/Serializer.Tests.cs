using Fauna.Serialization;
using Fauna.Serialization.Attributes;
using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

[TestFixture]
public class SerializerTests
{
    
    private class TestPerson
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int Age { get; set; }
    }
    
    [FaunaObject]
    private class TestPersonWithAttributes
    {
        [Field("first_name")]
        public string? FirstName { get; set; }
        [Field("last_name")]
        public string? LastName { get; set; }
        [Field("age")]
        public int Age { get; set; }
        public string? Ignored { get; set; }
    }

    
    [Test]
    public void DeserializeValues()
    {
        var tests = new Dictionary<string, object?>
        {
            {"\"hello\"", "hello"},
            {"""{"@int":"42"}""", 42},
            {"""{"@long":"42"}""", 42L},
            {"""{"@double": "1.2"}""", 1.2d},
            {"""{"@date": "2023-12-03"}""", new DateTime(2023, 12, 3)},
            {"""{"@time": "2023-12-03T05:52:10.000001-09:00"}""", new DateTime(2023, 12, 3, 14, 52, 10, 0, 1, DateTimeKind.Utc).ToLocalTime()},
            {"true", true},
            {"false", false},
            {"null", null},
        };
        
        foreach(KeyValuePair<string, object?> entry in tests)
        {
            var result = Serializer.Deserialize(entry.Key);
            Assert.AreEqual(entry.Value, result);
        }
    }
    
    [Test]
    public void DeserializeObject()
    {
        const string given = """
                             {
                                "aString": "foo",
                                "anObject": { "baz": "luhrmann" },
                                "anInt": { "@int": "2147483647" },
                                "aLong":{ "@long": "9223372036854775807" },
                                "aDouble":{ "@double": "3.14159" },
                                "aDate":{ "@date": "2023-12-03" },
                                "aTime":{ "@time": "2023-12-03T14:52:10.001001Z" },
                                "true": true,
                                "false": false,
                                "null": null
                             }
                             """;

        var inner = new Dictionary<string, object>
        {
            { "baz",  "luhrmann" }
        };
        
        var expected = new Dictionary<string, object?>
        {
            { "aString", "foo" },
            { "anObject", inner },
            { "anInt", 2147483647 },
            { "aLong", 9223372036854775807 },
            { "aDouble", 3.14159d },
            { "aDate", new DateTime(2023, 12, 3) },
            { "aTime", new DateTime(2023, 12, 3, 14, 52, 10, 1, 1, DateTimeKind.Utc).ToLocalTime() },
            { "true", true },
            { "false", false },
            { "null", null }
        };
            
        var result = Serializer.Deserialize(given);
        Assert.AreEqual(expected, result);
    }
    
    [Test]
    public void DeserializeEscapedObject()
    {
        const string given = """
                             {
                                "@object": {
                                    "@int": "notanint",
                                    "anInt": { "@int": "123" },
                                    "@object": "notanobject",
                                    "anEscapedObject": { "@object": { "@long": "notalong" } }
                                }
                             }
                             """;

        var inner = new Dictionary<string, object>
        {
            { "@long",  "notalong" }
        };
        
        var expected = new Dictionary<string, object>
        {
            { "@int", "notanint" },
            { "anInt", 123 },
            { "@object", "notanobject" },
            { "anEscapedObject", inner }
            
        };
            
        var result = Serializer.Deserialize(given);
        Assert.AreEqual(expected, result);
    }
    
    
    [Test]
    public void DeserializeIntoPoco()
    {
        
        const string given = """
                             {
                                "FirstName": "Baz",
                                "LastName": "Luhrmann",
                                "Age": { "@int": "61" }
                             }
                             """;
        
        var p = Serializer.Deserialize<TestPerson>(given);
        Assert.AreEqual("Baz", p.FirstName);
        Assert.AreEqual("Luhrmann", p.LastName);
        Assert.AreEqual(61, p.Age);
    }
    
    [Test]
    public void DeserializeIntoPocoWithAttributes()
    {
        const string given = """
                             {
                                "first_name": "Baz",
                                "last_name": "Luhrmann",
                                "age": { "@int": "61" },
                                "Ignored": "should be null"
                             }
                             """;
        
        var p = Serializer.Deserialize<TestPersonWithAttributes>(given);
        Assert.AreEqual("Baz", p.FirstName);
        Assert.AreEqual("Luhrmann", p.LastName);
        Assert.AreEqual(61, p.Age);
        Assert.IsNull(p.Ignored);
    }
}