using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using static Fauna.Query;
using static Fauna.Test.Helpers.TestClientHelper;

namespace Fauna.Test.Linq;

[TestFixture]
public class QueryTests
{

    [AllowNull]
    private static Client client;
    [AllowNull]
    private static AuthorDb db;

    [OneTimeSetUp]
    public void SetUp()
    {
        client = NewTestClient();
        db = Fixtures.AuthorDb(client);
    }

    [Test]
    public async Task Collection_PaginateAsync()
    {
        await foreach (var p in db.Author.PaginateAsync())
        {
            var names = p.Data.Select(a => a.Name);
            Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
        }
    }

    [Test]
    public async Task Collection_AsyncEnumerable()
    {
        var names = new List<string>();
        await foreach (var a in db.Author.AsAsyncEnumerable())
        {
            names.Add(a.Name);
        }
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }

    [Test]
    public async Task Index_PaginateAsync()
    {
        await foreach (var p in db.Author.ByName("Alice").PaginateAsync())
        {
            var names = p.Data.Select(a => a.Name);
            Assert.AreEqual(new List<string> { "Alice" }, names);
        }
    }

    [Test]
    public async Task Index_AsyncEnumerable()
    {
        var names = new List<string>();
        await foreach (var a in db.Author.ByName("Alice").AsAsyncEnumerable())
        {
            names.Add(a.Name);
        }
        Assert.AreEqual(new List<string> { "Alice" }, names);
    }
}
