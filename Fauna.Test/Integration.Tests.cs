using Fauna.Mapping.Attributes;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using static Fauna.Query;
using static Fauna.Test.Helpers.TestClientHelper;

namespace Fauna.Test;

[TestFixture]
[Ignore("integ test")]
public class IntegrationTests
{
    [AllowNull]
    private static Client _client;

    [Object]
    private class Person
    {
        [Field("first_name")]
        public string? FirstName { get; set; }
        [Field("last_name")]
        public string? LastName { get; set; }
        [Field("age")]
        public int Age { get; set; }
    }

    [OneTimeSetUp]
    public void SetUp()
    {
        _client = NewTestClient();
    }

    [Test]
    public async Task UserDefinedObjectTest()
    {
        var expected = new Person
        {
            FirstName = "Georgia",
            LastName = "O'Keeffe",
            Age = 136
        };
        var query = FQL($"{expected}");
        var result = await _client.QueryAsync<Person>(query);
        var actual = result.Data;

        Assert.AreNotEqual(expected, actual);
        Assert.AreEqual(expected.FirstName, actual.FirstName);
        Assert.AreEqual(expected.LastName, actual.LastName);
        Assert.AreEqual(expected.Age, actual.Age);
    }

    [Test]
    public async Task Paginate_SinglePageWithSmallCollection()
    {
        var query = FQL($@"[1,2,3,4,5,6,7,8,9,10].toSet();");

        var paginatedResult = _client.PaginateAsync<int>(query);

        int pageCount = 0;
        await foreach (var page in paginatedResult)
        {
            pageCount++;
            var data = page.Data;
            Assert.AreEqual(10, data.Count());
        }

        Assert.AreEqual(1, pageCount);
    }

    [Test]
    public async Task Paginate_MultiplePagesWithCollection()
    {
        var query = FQL($@"[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20].toSet()");

        var paginatedResult = _client.PaginateAsync<int>(query);

        int pageCount = 0;
        await foreach (var page in paginatedResult)
        {
            pageCount++;
            var data = page.Data;
            Assert.AreEqual(10, data.Count());
        }

        Assert.AreEqual(2, pageCount);
    }

    [Test]
    public async Task Paginate_MultiplePagesWithPocoCollection()
    {
        var items = new List<object>();
        for (int i = 1; i <= 100; i++)
        {
            items.Add(new Person
            {
                FirstName = $"FirstName{i}",
                LastName = $"LastName{i}",
                Age = i
            });
        }

        var query = FQL($"{items}.toSet()");

        var paginatedResult = _client.PaginateAsync<Person>(query);

        int pageCount = 0;
        await foreach (var page in paginatedResult)
        {
            pageCount++;
            var data = page.Data;
            Assert.AreEqual(20, data.Count());
        }

        Assert.AreEqual(5, pageCount);
    }

    [Test]
    public async Task Paginate_IteratorCanBeFlattened()
    {
        var items = new List<object>();
        for (int i = 1; i <= 100; i++)
        {
            items.Add(new Person
            {
                FirstName = $"FirstName{i}",
                LastName = $"LastName{i}",
                Age = i
            });
        }

        var query = FQL($"{items}.toSet()");

        var paginatedResult = _client.PaginateAsync<Person>(query);

        int itemCount = 0;
        await foreach (var item in paginatedResult.FlattenAsync<Person>())
        {
            itemCount++;
        }

        Assert.AreEqual(100, itemCount);
    }
}
