using Fauna.Constants;
using Fauna.Serialization.Attributes;
using NUnit.Framework;
using static Fauna.Query;

namespace Fauna.Test;

[TestFixture]
public class IntegrationTests
{
    [FaunaObject]
    private class Person
    {
        [Field("first_name")]
        public string? FirstName { get; set; }
        [Field("last_name")]
        public string? LastName { get; set; }
        [Field("age")]
        public int Age { get; set; }
    }

    [Test]
    [Ignore("integ test")]
    public async Task UserDefinedObjectTest()
    {
        var expected = new Person
        {
            FirstName = "Georgia",
            LastName = "O'Keeffe",
            Age = 136
        };
        var config = new ClientConfig("secret") { Endpoint = Endpoints.Local, ConnectionTimeout = TimeSpan.FromSeconds(30) };
        using var client = new Client(config);
        var query = FQL($"{expected}");
        var result = await client.QueryAsync<Person>(query);
        var actual = result.Data;

        Assert.AreNotEqual(expected, actual);
        Assert.AreEqual(expected.FirstName, actual.FirstName);
        Assert.AreEqual(expected.LastName, actual.LastName);
        Assert.AreEqual(expected.Age, actual.Age);
    }

    [Test]
    [Ignore("integ test")]
    public async Task UserDefinedObjectTest_WithExternalHttpClient()
    {
        var expected = new Person
        {
            FirstName = "Georgia",
            LastName = "O'Keeffe",
            Age = 136
        };

        var config = new ClientConfig("secret") { Endpoint = Endpoints.Local, ConnectionTimeout = TimeSpan.FromSeconds(30) };
        using var httpClient = new HttpClient();
        using var client = new Client(httpClient, config);
        var query = FQL($"{expected}");
        var result = await client.QueryAsync<Person>(query);
        var actual = result.Data;

        Assert.AreNotEqual(expected, actual);
        Assert.AreEqual(expected.FirstName, actual.FirstName);
        Assert.AreEqual(expected.LastName, actual.LastName);
        Assert.AreEqual(expected.Age, actual.Age);
    }

}