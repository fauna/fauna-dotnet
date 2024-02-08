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

    [Test]
    public async Task Query_Select_Field()
    {
        var names = new List<string>();
        await foreach (var n in db.Author.Select(a => a.Name).AsAsyncEnumerable())
        {
            names.Add(n);
        }
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }

    private (string, string) Escaper(Author a) =>
        (a.Name, new String(a.Name.Reverse().ToArray()));

    [Test]
    public async Task Query_Select_Projected()
    {
        var names = new List<string>();
        await foreach (var obj in db.Author.Select(a => new { a.Name }).AsAsyncEnumerable())
        {
            names.Add(obj.Name);
        }
        Assert.AreEqual(new List<string> { "Alice", "Bob", }, names);
    }

    [Test]
    public async Task Query_Select_Escaped()
    {
        var names = new List<string>();
        await foreach (var (n, rn) in db.Author.Select(a => Escaper(a)).AsAsyncEnumerable())
        {
            names.Add(n);
            names.Add(rn);
        }
        Assert.AreEqual(new List<string> { "Alice", "ecilA", "Bob", "boB" }, names);
    }

    [Test]
    public async Task Query_Reverse()
    {
        var names = new List<string>();
        await foreach (var a in db.Author.Reverse().AsAsyncEnumerable())
        {
            names.Add(a.Name);
        }
        Assert.AreEqual(new List<string> { "Bob", "Alice" }, names);
    }

    [Test]
    public void Query_ToHashSet()
    {
        var hset = db.Author.ToHashSet();
        var names = hset.Select(a => a.Name);
        Assert.AreEqual(new string[] { "Alice", "Bob" }.ToHashSet(), names);
    }
}
