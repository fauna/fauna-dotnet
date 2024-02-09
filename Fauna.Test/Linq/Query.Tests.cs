using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
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
        await foreach (var a in db.Author.ToAsyncEnumerable())
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
        await foreach (var a in db.Author.ByName("Alice").ToAsyncEnumerable())
        {
            names.Add(a.Name);
        }
        Assert.AreEqual(new List<string> { "Alice" }, names);
    }

    [Test]
    public async Task Query_Where()
    {
        var names = new List<string>();
        await foreach (var a in db.Author.Where(a => a.Name == "Alice").ToAsyncEnumerable())
        {
            names.Add(a.Name);
        }
        Assert.AreEqual(new List<string> { "Alice" }, names);
    }

    [Test]
    public async Task Query_Select_Field()
    {
        var names = new List<string>();
        await foreach (var n in db.Author.Select(a => a.Name).ToAsyncEnumerable())
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
        await foreach (var obj in db.Author.Select(a => new { a.Name }).ToAsyncEnumerable())
        {
            names.Add(obj.Name);
        }
        Assert.AreEqual(new List<string> { "Alice", "Bob", }, names);
    }

    [Test]
    public async Task Query_Select_Escaped()
    {
        var names = new List<string>();
        await foreach (var (n, rn) in db.Author.Select(a => Escaper(a)).ToAsyncEnumerable())
        {
            names.Add(n);
            names.Add(rn);
        }
        Assert.AreEqual(new List<string> { "Alice", "ecilA", "Bob", "boB" }, names);
    }

    [Test]
    public void Query_Any()
    {
        var any = db.Author.Any();
        Assert.AreEqual(true, any);

        var anyPred = db.Author.Any(a => a.Name == "Bob");
        Assert.AreEqual(true, anyPred);
    }

    [Test]
    public void Query_All()
    {
        var all = db.Author.All(a => a.Name == "Bob");
        Assert.AreEqual(false, all);
    }

    [Test]
    public void Query_Count()
    {
        var count = db.Author.Count();
        Assert.AreEqual(2, count);
    }

    [Test]
    public async Task Query_Distinct()
    {
        var ages = new List<int>();
        await foreach (var n in db.Author.Select(a => a.Age).Distinct().ToAsyncEnumerable())
        {
            ages.Add(n);
        }
        Assert.AreEqual(new List<int> { 26, 32 }, ages);
    }

    [Test]
    public void Query_First()
    {
        var fst = db.Author.First();
        Assert.AreEqual("Alice", fst.Name);

        var fstPred = db.Author.First(a => a.Name == "Bob");
        Assert.AreEqual("Bob", fstPred.Name);
    }

    [Test]
    [Ignore("broken deserialization")]
    public void Query_FirstOrDefault()
    {
        var fst = db.Author.FirstOrDefault();
        Assert.AreEqual("Alice", fst?.Name);

        var fstPred = db.Author.FirstOrDefault(a => a.Name == "Bob");
        Assert.AreEqual("Bob", fstPred?.Name);

        var fstNull = db.Author.FirstOrDefault(a => a.Name == "No name");
        Assert.AreEqual(null, fstNull);
    }

    [Test]
    public void Query_Last()
    {
        var lst = db.Author.Last();
        Assert.AreEqual("Bob", lst.Name);

        var lstPred = db.Author.Last(a => a.Name == "Alice");
        Assert.AreEqual("Alice", lstPred.Name);
    }

    [Test]
    [Ignore("broken deserialization")]
    public void Query_LastOrDefault()
    {
        var lst = db.Author.LastOrDefault();
        Assert.AreEqual("Bob", lst?.Name);

        var lstPred = db.Author.LastOrDefault(a => a.Name == "Alice");
        Assert.AreEqual("Alice", lstPred?.Name);

        var lstNull = db.Author.LastOrDefault(a => a.Name == "No name");
        Assert.AreEqual(null, lstNull);
    }

    // [Test]
    // [Ignore("broken deserialization")]
    // public void Query_LongCount()
    // {
    //     var count = db.Author.LongCount();
    //     Assert.AreEqual(2L, count);
    // }

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
    public async Task Query_Order()
    {
        var names = new List<string>();
        await foreach (var a in db.Author.Reverse().Order().ToAsyncEnumerable())
        {
            names.Add(a.Name);
        }
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }

    [Test]
    public async Task Query_OrderBy()
    {
        var names = new List<string>();
        await foreach (var a in db.Author.OrderBy(a => a.Age).ToAsyncEnumerable())
        {
            names.Add(a.Name);
        }
        Assert.AreEqual(new List<string> { "Bob", "Alice" }, names);
    }

    [Test]
    public async Task Query_OrderDescending()
    {
        var names = new List<string>();
        await foreach (var a in db.Author.OrderDescending().ToAsyncEnumerable())
        {
            names.Add(a.Name);
        }
        Assert.AreEqual(new List<string> { "Bob", "Alice" }, names);
    }

    [Test]
    public async Task Query_OrderByDescending()
    {
        var names = new List<string>();
        await foreach (var a in db.Author.OrderByDescending(a => a.Name).ToAsyncEnumerable())
        {
            names.Add(a.Name);
        }
        Assert.AreEqual(new List<string> { "Bob", "Alice" }, names);
    }

    [Test]
    public async Task Query_Reverse()
    {
        var names = new List<string>();
        await foreach (var a in db.Author.Reverse().ToAsyncEnumerable())
        {
            names.Add(a.Name);
        }
        Assert.AreEqual(new List<string> { "Bob", "Alice" }, names);
    }

    [Test]
    public async Task Query_Skip()
    {
        var names = new List<string>();
        await foreach (var a in db.Author.Skip(1).ToAsyncEnumerable())
        {
            names.Add(a.Name);
        }
        Assert.AreEqual(new List<string> { "Bob" }, names);
    }

    [Test]
    public void Query_Sum()
    {
        var sum = db.Author.Select(a => a.Age).Sum();
        Assert.AreEqual(58, sum);
    }

    [Test]
    public async Task Query_Take()
    {
        var names = new List<string>();
        await foreach (var a in db.Author.Take(1).ToAsyncEnumerable())
        {
            names.Add(a.Name);
        }
        Assert.AreEqual(new List<string> { "Alice" }, names);
    }
}
