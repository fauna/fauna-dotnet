using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

public class PageSerializer_Tests
{
    private static readonly MappingContext s_ctx = new();


    [Test]
    public void DeserializeIntoPageWithPrimitive()
    {
        const string wire = @"{
            ""@set"": {
                ""after"": ""next_page_cursor"",
                ""data"": [
                    {""@int"":""1""},
                    {""@int"":""2""},
                    {""@int"":""3""}
                ]
            }
        }";

        var serializer = Serializer.Generate<Page<int>>(s_ctx);
        var deserialized = Helpers.Deserialize(serializer, s_ctx, wire)!;
        Assert.AreEqual(new List<int> { 1, 2, 3 }, deserialized.Data);
        Assert.AreEqual("next_page_cursor", deserialized.After);
    }

    [Test]
    public void DeserializeIntoPageWithSingleValue()
    {
        const string wire = @"""SingleValue""";
        var serializer = Serializer.Generate<Page<string>>(s_ctx);
        var deserialized = Helpers.Deserialize(serializer, s_ctx, wire)!;
        Assert.AreEqual(new List<string> { "SingleValue" }, deserialized.Data);
        Assert.IsNull(deserialized.After);
    }

    [Test]
    public void DeserializeIntoPageWithUserDefinedClass()
    {
        const string wire = @"{
            ""@set"": {
                ""after"": ""next_page_cursor"",
                ""data"": [
                    {""first_name"":""Alice"",""last_name"":""Smith"",""age"":{""@int"":""30""}},
                    {""first_name"":""Bob"",""last_name"":""Jones"",""age"":{""@int"":""40""}}
                ]
            }
        }";

        var serializer = Serializer.Generate<Page<PersonWithAttributes>>(s_ctx);
        var deserialized = Helpers.Deserialize(serializer, s_ctx, wire)!;
        Assert.AreEqual(2, deserialized.Data.Count);
        Assert.AreEqual("Alice", deserialized.Data[0].FirstName);
        Assert.AreEqual("Smith", deserialized.Data[0].LastName);
        Assert.AreEqual(30, deserialized.Data[0].Age);
        Assert.AreEqual("Bob", deserialized.Data[1].FirstName);
        Assert.AreEqual("Jones", deserialized.Data[1].LastName);
        Assert.AreEqual(40, deserialized.Data[1].Age);
        Assert.AreEqual("next_page_cursor", deserialized.After);
    }
}
