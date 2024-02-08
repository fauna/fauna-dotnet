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
    public async Task Query_Where()
    {
        var names = new List<string>();
        await foreach (var a in db.Author.Where(a => a.Name == "Alice").AsAsyncEnumerable())
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
    public void Query_Count()
    {
        var count = db.Author.Count();
        Assert.AreEqual(2, count);
    }

    [Test]
    [Ignore("broken deserialization")]
    public void Query_LongCount()
    {
        var count = db.Author.LongCount();
        Assert.AreEqual(2, count);
    }

    [Test]
    public void Query_First()
    {
        var fst = db.Author.First();
        Assert.AreEqual("Alice", fst.Name);
    }

    [Test]
    public void Query_FirstWithPred()
    {
        var fst = db.Author.First(a => a.Name == "Bob");
        Assert.AreEqual("Bob", fst.Name);
    }

    [Test]
    public void Query_Last()
    {
        var fst = db.Author.First();
        Assert.AreEqual("Alice", fst.Name);
    }

    [Test]
    public void Query_AnyIsEmpty()
    {
        var any = db.Author.Any();
        Assert.AreEqual(true, any);
    }

    [Test]
    public void Query_AnyWithPred()
    {
        var any = db.Author.Any(a => a.Name == "Bob");
        Assert.AreEqual(true, any);
    }

    [Test]
    public void Query_All()
    {
        var all = db.Author.All(a => a.Name == "Bob");
        Assert.AreEqual(false, all);
    }

    [Test]
    [Ignore("unimplemented")]
    public void Query_Average()
    {
        var avg = db.Author.Select(a => a.Age).Average();
        Assert.AreEqual(29, avg);
    }

    [Test]
    public void Query_Max()
    {
        var max = db.Author.Select(a => a.Age).Max();
        Assert.AreEqual(32, max);
    }

    [Test]
    public void Query_Min()
    {
        var min = db.Author.Select(a => a.Age).Min();
        Assert.AreEqual(26, min);
    }

    [Test]
    public void Query_Sum()
    {
        var sum = db.Author.Select(a => a.Age).Sum();
        Assert.AreEqual(58, sum);
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
