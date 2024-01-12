using Fauna.Constants;
using Fauna.Serialization.Attributes;
using NUnit.Framework;
using static Fauna.Query;

namespace Fauna.Test;

[TestFixture]
//[Ignore("integ test")]
public class IntegrationTests
{
#pragma warning disable CS8618
    private static Client _client;
#pragma warning restore CS8618

    private const int DefaultSetSize = 16;

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

    [OneTimeSetUp]
    public void SetUp()
    {
        var connection = new Connection(Endpoints.Default, TimeSpan.FromSeconds(5), 3, TimeSpan.FromSeconds(10));
        _client = new Client(new ClientConfig("fnAFXbBvA8AAyjXFFuxEBPVU34dlA1CPbYfBAZRQ"), connection);
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
        var query = FQL($@"[1,2,3,4,5,6,7,8,9,10].toSet().paginate(10);");

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
        var query = FQL($@"[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20].toSet().paginate(10);");

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
    public async Task Paginate_Set()
    {
        int expectedTotalItemsCount = 18;
        var expectedPageCounts = new[] { DefaultSetSize, expectedTotalItemsCount - DefaultSetSize };

        var query = FQL($@"[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18].toSet();");
        var paginatedResult = _client.PaginateAsync<int>(query);

        int pageNumber = 0;
        await foreach (var page in paginatedResult)
        {
            var expectedCount = expectedPageCounts[pageNumber++];
            Assert.AreEqual(expectedCount, page.Data.Count);
        }

        Assert.AreEqual(expectedPageCounts.Length, pageNumber);
    }

    [Test]
    public async Task Paginate_EmbeddedSet()
    {
        int expectedTotalItemsCount = 20;
        int expectedFirstPageItemsCount = DefaultSetSize;
        int expectedSecondPageItemsCount = expectedTotalItemsCount - expectedFirstPageItemsCount;

        var query = FQL($@"[[1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20].toSet(),2,3].toSet();");

        var paginatedResult = _client.PaginateAsync<object>(query);

        int pageCount = 0;
        await foreach (Page<object> page in paginatedResult)
        {
            pageCount++;
            Assert.IsNotNull(page.Data);
            Assert.AreEqual(3, page.Data.Count);

            var embeddedPage = page.Data[0] as Page<object>;
            Assert.IsNotNull(embeddedPage);
            int totalItemsCount = embeddedPage!.Data.Count;
            Assert.AreEqual(expectedFirstPageItemsCount, totalItemsCount);

            var embeddedPageResult = _client.PaginateAsync(embeddedPage);

            await foreach (Page<object> subpage in embeddedPageResult)
            {

                Assert.IsNotNull(subpage.Data);
                Assert.AreEqual(expectedSecondPageItemsCount, subpage.Data.Count);

                totalItemsCount += subpage.Data.Count;
            }

            Assert.AreEqual(expectedTotalItemsCount, totalItemsCount);
        }
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

        var query = FQL($"{items}.toSet().paginate(20);");

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
    public async Task PaginateAsync_NonPageResultYieldsOnePocoItem()
    {
        var expected = new Person
        {
            FirstName = "Georgia",
            LastName = "O'Keeffe",
            Age = 136
        };
        var query = FQL($"{expected}");

        var paginatedResult = _client.PaginateAsync<Person>(query);

        var pageCount = 0;

        await foreach (var page in paginatedResult)
        {
            pageCount++;

            Assert.IsNotNull(page);
            Assert.IsNull(page.After);
            Assert.That(page.Data, Has.Count.EqualTo(1));

            var actual = page.Data[0];

            Assert.That(actual, Is.Not.EqualTo(expected));
            Assert.AreEqual(expected.FirstName, actual.FirstName);
            Assert.AreEqual(expected.LastName, actual.LastName);
            Assert.AreEqual(expected.Age, actual.Age);
        }

        Assert.AreEqual(1, pageCount);
    }

    [Test]
    public async Task PaginateAsync_NonPageResultYieldsOneIntItem()
    {
        var expected = 125;
        var query = FQL($"{expected}");

        var paginatedResult = _client.PaginateAsync<int>(query);

        var pageCount = 0;

        await foreach (var page in paginatedResult)
        {
            pageCount++;

            Assert.IsNotNull(page);
            Assert.IsNull(page.After);
            Assert.That(page.Data, Has.Count.EqualTo(1));

            var actual = page.Data[0];

            Assert.AreEqual(actual, expected);
            Assert.AreEqual(expected, actual);
        }

        Assert.AreEqual(1, pageCount);
    }

    [Test]
    public async Task PaginateAsync_NonPageResultYieldsOneCollectionItem()
    {
        var query = FQL($"[1,2,3]");

        var paginatedResult = _client.PaginateAsync<List<int>>(query);

        var pageCount = 0;

        await foreach (var page in paginatedResult)
        {
            pageCount++;

            Assert.IsNotNull(page);
            Assert.IsNull(page.After);
            Assert.That(page.Data, Has.Count.EqualTo(1));

            var actual = page.Data[0];
            Assert.That(actual, Has.Count.EqualTo(3));
            Assert.AreEqual(1, actual[0]);
            Assert.AreEqual(2, actual[1]);
            Assert.AreEqual(3, actual[2]);
        }

        Assert.AreEqual(1, pageCount);
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

        var query = FQL($"{items}.toSet().paginate(20);");

        var paginatedResult = _client.PaginateAsync<Person>(query);

        int itemCount = 0;
        await foreach (var item in paginatedResult.FlattenAsync<Person>())
        {
            itemCount++;
        }

        Assert.AreEqual(100, itemCount);
    }
}