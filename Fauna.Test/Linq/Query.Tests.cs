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

    [OneTimeTearDown]
    public void TearDown()
    {
        client.Dispose();
    }

    [Test]
    public async Task Collection_PaginateAsync()
    {
        var names = new List<string>();
        await foreach (var p in db.Author.PaginateAsync())
        {
            names.AddRange(p.Data.Select(a => a.Name));
        }
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }


    [Test]
    public async Task Index_PaginateAsync()
    {
        var names = new List<string>();
        await foreach (var p in db.Author.ByName("Alice").PaginateAsync())
        {
            names.AddRange(p.Data.Select(a => a.Name));
        }
        Assert.AreEqual(new List<string> { "Alice" }, names);
    }

    [Test]
    public async Task Query_Cancellation()
    {
        async Task TestCancel(string method, Func<CancellationToken, Task> testfn)
        {
            using var cancel = new CancellationTokenSource();
            cancel.Cancel();
            try
            {
                await testfn(cancel.Token);
                Assert.Fail($"{method} was not cancelled");
            }
            catch (System.Threading.Tasks.TaskCanceledException) { }
        }

        await TestCancel("PaginateAsync", async (c) =>
        {
            await foreach (var p in db.Author.PaginateAsync(cancel: c))
            {
                Console.WriteLine(p.Data);
            }
        });
        await TestCancel("ToAsyncEnumerable", async (c) =>
        {
            await foreach (var a in db.Author.ToAsyncEnumerable(cancel: c))
            {
                Console.WriteLine(a);
            }
        });

        await TestCancel("AllAsync", async (c) => await db.Author.AllAsync(d => true, c));
        await TestCancel("AnyAsync", async (c) => await db.Author.AnyAsync(c));
        await TestCancel("AnyAsync", async (c) => await db.Author.AnyAsync(d => true, c));
        await TestCancel("CountAsync", async (c) => await db.Author.CountAsync(c));
        await TestCancel("CountAsync", async (c) => await db.Author.CountAsync(d => true, c));
        await TestCancel("FirstAsync", async (c) => await db.Author.FirstAsync(c));
        await TestCancel("FirstAsync", async (c) => await db.Author.FirstAsync(d => true, c));
        await TestCancel("FirstOrDefaultAsync", async (c) => await db.Author.FirstOrDefaultAsync(c));
        await TestCancel("FirstOrDefaultAsync", async (c) => await db.Author.FirstOrDefaultAsync(d => true, c));
        await TestCancel("LastAsync", async (c) => await db.Author.LastAsync(c));
        await TestCancel("LastAsync", async (c) => await db.Author.LastAsync(d => true, c));
        await TestCancel("LastOrDefaultAsync", async (c) => await db.Author.LastOrDefaultAsync(c));
        await TestCancel("LastOrDefaultAsync", async (c) => await db.Author.LastOrDefaultAsync(d => true, c));
        await TestCancel("LongCountAsync", async (c) => await db.Author.LongCountAsync(c));
        await TestCancel("LongCountAsync", async (c) => await db.Author.LongCountAsync(d => true, c));
        await TestCancel("MaxAsync", async (c) => await db.Author.MaxAsync(c));
        await TestCancel("MaxAsync", async (c) => await db.Author.MaxAsync(d => true, c));
        await TestCancel("MinAsync", async (c) => await db.Author.MinAsync(c));
        await TestCancel("MinAsync", async (c) => await db.Author.MinAsync(d => true, c));
        await TestCancel("SumAsync", async (c) => await db.Author.SumAsync(d => (int)1, c));
        await TestCancel("SumAsync", async (c) => await db.Author.SumAsync(d => (long)1L, c));
        await TestCancel("SumAsync", async (c) => await db.Author.SumAsync(d => (double)1D, c));
        await TestCancel("AverageAsync", async (c) => await db.Author.AverageAsync(d => (int)1, c));
        await TestCancel("AverageAsync", async (c) => await db.Author.AverageAsync(d => (long)1L, c));
        await TestCancel("AverageAsync", async (c) => await db.Author.AverageAsync(d => (double)1D, c));

        await TestCancel("ToListAsync", async (c) => await db.Author.ToListAsync(c));
        await TestCancel("ToArrayAsync", async (c) => await db.Author.ToArrayAsync(c));
        await TestCancel("ToHashSetAsync", async (c) => await db.Author.ToHashSetAsync(c));
        await TestCancel("ToDictionaryAsync", async (c) =>
            await db.Author.ToDictionaryAsync(a => a.Name, a => a.Age, c));
    }

    [Test]
    public async Task Query_ToAsyncEnumerable()
    {
        var names = new List<string>();
        await foreach (var a in db.Author.ToAsyncEnumerable())
        {
            names.Add(a.Name);
        }
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }

    [Test]
    public void Query_ToEnumerable()
    {
        var names = new List<string>();
        foreach (var a in db.Author.ToEnumerable())
        {
            names.Add(a.Name);
        }
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }

    [Test]
    public async Task Query_ToListAsync()
    {
        var names = await db.Author.Select(a => a.Name).ToListAsync();
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }

    [Test]
    public void Query_ToList()
    {
        var names = db.Author.Select(a => a.Name).ToList();
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }

    [Test]
    public async Task Query_ToArrayAsync()
    {
        var names = await db.Author.Select(a => a.Name).ToArrayAsync();
        Assert.AreEqual(new string[] { "Alice", "Bob" }, names);
    }

    [Test]
    public void Query_ToArray()
    {
        var names = db.Author.Select(a => a.Name).ToArray();
        Assert.AreEqual(new string[] { "Alice", "Bob" }, names);
    }

    [Test]
    public async Task Query_ToHashSetAsync()
    {
        var names = await db.Author.Select(a => a.Name).ToHashSetAsync();
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }

    [Test]
    public void Query_ToHashSet()
    {
        var names = db.Author.Select(a => a.Name).ToHashSet();
        Assert.AreEqual(new HashSet<string> { "Alice", "Bob" }, names);
    }

    [Test]
    public async Task Query_ToDictionaryAsyncSelector()
    {
        var dict = await db.Author.ToDictionaryAsync(a => a.Name, a => a.Age);
        Assert.AreEqual(new Dictionary<string, int> { { "Alice", 32 }, { "Bob", 26 } }, dict);
    }

    [Test]
    public void Query_ToDictionarySelector()
    {
        var dict = db.Author.ToDictionary(a => a.Name, a => a.Age);
        Assert.AreEqual(new Dictionary<string, int> { { "Alice", 32 }, { "Bob", 26 } }, dict);
    }

    [Test]
    public async Task Query_ToDictionaryAsync()
    {
        var dict = await db.Author.Select(a => ValueTuple.Create(a.Name, a.Age)).ToDictionaryAsync();
        Assert.AreEqual(new Dictionary<string, int> { { "Alice", 32 }, { "Bob", 26 } }, dict);
    }

    [Test]
    public void Query_ToDictionary()
    {
        var dict = db.Author.Select(a => ValueTuple.Create(a.Name, a.Age)).ToDictionary();
        Assert.AreEqual(new Dictionary<string, int> { { "Alice", 32 }, { "Bob", 26 } }, dict);
    }

    [Test]
    public async Task Query_ExpressionSyntax()
    {
        var q1 = from a in db.Author
                 where a.Name == "Alice"
                 select a.Age;

        Assert.AreEqual(new List<int> { 32 }, await q1.ToListAsync());

        var q2 = from a in db.Author.ByName("Alice")
                 select a.Age;

        Assert.AreEqual(new List<int> { 32 }, await q2.ToListAsync());

        var q3 = from a in db.Author
                 orderby a.Age
                 select a.Name;

        Assert.AreEqual(new List<string> { "Bob", "Alice" }, await q3.ToListAsync());
    }

    [Test]
    public async Task Query_Where()
    {
        var authors = await db.Author.Where(a => a.Name == "Alice").ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Alice" }, names);
    }

    [Test]
    public async Task Query_WhereQuery_ClosureRef()
    {
        var name = "Alice";
        var authors = await db.Author.Where(a => a.Name == name).ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Alice" }, names);
    }

    static readonly string _aliceName = "Alice";

    [Test]
    public async Task Query_WhereQuery_ConstantField()
    {
        var authors = await db.Author.Where(a => a.Name == _aliceName).ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Alice" }, names);
    }

    static string AliceName { get; } = "Alice";

    [Test]
    public async Task Query_WhereQuery_ConstantProp()
    {
        var authors = await db.Author.Where(a => a.Name == AliceName).ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Alice" }, names);
    }

    static string AliceNameVirt { get => "Alice"; }

    [Test]
    public async Task Query_WhereQuery_ConstantVirtualProp()
    {
        var authors = await db.Author.Where(a => a.Name == AliceNameVirt).ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Alice" }, names);
    }

    [Test]
    public async Task Query_Select_Field()
    {
        var names = await db.Author.Select(a => a.Name).ToListAsync();
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }

    [Test]
    public async Task Query_Select_Projected()
    {
        var obj = await db.Author.Select(a => new { a.Name }).ToListAsync();
        var names = obj.Select(o => o.Name);
        Assert.AreEqual(new List<string> { "Alice", "Bob", }, names);
    }

    private (string, string) Escaper(Author a) =>
        (a.Name, new String(a.Name.Reverse().ToArray()));

    [Test]
    public async Task Query_Select_Escaped()
    {
        var tups = await db.Author.Select(a => Escaper(a)).ToListAsync();
        Assert.AreEqual(new List<(string, string)> { ("Alice", "ecilA"), ("Bob", "boB") }, tups);
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
        var ages = await db.Author.Select(a => a.Age).Distinct().ToListAsync();
        Assert.AreEqual(new List<int> { 26, 32 }, ages);
    }

    [Test]
    public void Query_First()
    {
        var fst = db.Author.First();
        Assert.AreEqual("Alice", fst.Name);

        var fstPred = db.Author.First(a => a.Name == "Bob");
        Assert.AreEqual("Bob", fstPred.Name);

        try
        {
            db.Author.First(a => a.Name == "No name");
            Assert.Fail();
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual("Empty set", ex.Message);
        }
    }

    [Test]
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

        try
        {
            db.Author.Last(a => a.Name == "No name");
            Assert.Fail();
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual("Empty set", ex.Message);
        }
    }

    [Test]
    public void Query_LastOrDefault()
    {
        var lst = db.Author.LastOrDefault();
        Assert.AreEqual("Bob", lst?.Name);

        var lstPred = db.Author.LastOrDefault(a => a.Name == "Alice");
        Assert.AreEqual("Alice", lstPred?.Name);

        var lstNull = db.Author.LastOrDefault(a => a.Name == "No name");
        Assert.AreEqual(null, lstNull);
    }

    [Test]
    public void Query_LongCount()
    {
        var count = db.Author.LongCount();
        Assert.AreEqual(2L, count);
    }

    [Test]
    public void Query_Max()
    {
        var max1 = db.Author.Select(a => a.Age).Max();
        Assert.AreEqual(32, max1);

        var max2 = db.Author.Max(a => a.Age);
        Assert.AreEqual(32, max2);

        try
        {
            db.Author.Where(a => a.Name == "No name").Max(a => a.Age);
            Assert.Fail();
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual("Empty set", ex.Message);
        }
    }

    [Test]
    public void Query_Min()
    {
        var min1 = db.Author.Select(a => a.Age).Min();
        Assert.AreEqual(26, min1);

        var min2 = db.Author.Min(a => a.Age);
        Assert.AreEqual(26, min2);

        try
        {
            db.Author.Where(a => a.Name == "No name").Min(a => a.Age);
            Assert.Fail();
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual("Empty set", ex.Message);
        }
    }

    [Test]
    public void Query_Average()
    {
        var avg1 = db.Author.Select(a => a.Age).Average();
        Assert.AreEqual(29, avg1);

        var avg1Long = db.Author.Select(a => a.Subscribers).Average();
        Assert.IsInstanceOf(typeof(long), avg1Long);
        Assert.AreEqual(155000000, avg1Long);

        var avg1Double = db.Author.Select(a => a.Score).Average();
        Assert.IsInstanceOf(typeof(double), avg1Double);
        Assert.AreEqual(87.400000000000006, avg1Double);

        var avg2 = db.Author.Average(a => a.Age);
        Assert.AreEqual(29, avg2);

        var avg2Long = db.Author.Average(a => a.Subscribers);
        Assert.IsInstanceOf(typeof(long), avg2Long);
        Assert.AreEqual(155000000, avg2Long);

        var avg2Double = db.Author.Average(a => a.Score);
        Assert.IsInstanceOf(typeof(double), avg2Double);
        Assert.AreEqual(87.400000000000006, avg2Double);


        var avgCastDouble = db.Author.Average(a => (double)a.Age);
        Assert.IsInstanceOf(typeof(double), avgCastDouble);
        Assert.AreEqual(29.0D, avgCastDouble);

        var avgCastLong = db.Author.Average(a => (long)a.Age);
        Assert.IsInstanceOf(typeof(long), avgCastLong);
        Assert.AreEqual(29L, avgCastLong);

        var avgCastInt = db.Author.Average(a => (int)a.Score);
        Assert.IsInstanceOf(typeof(int), avgCastInt);
        Assert.AreEqual(87, avgCastInt);

        Assert.Throws<InvalidOperationException>(() => db.Author.Where(a => a.Name == "No name").Average(a => a.Age));
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
        var authors = await db.Author.OrderBy(a => a.Age).ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Bob", "Alice" }, names);
    }

    [Test]
    public async Task Query_OrderDescending()
    {
        var authors = await db.Author.OrderDescending().ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Bob", "Alice" }, names);
    }

    [Test]
    public async Task Query_OrderByDescending()
    {
        var authors = await db.Author.OrderByDescending(a => a.Name).ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Bob", "Alice" }, names);
    }

    [Test]
    public async Task Query_Reverse()
    {
        var authors = await db.Author.Reverse().ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Bob", "Alice" }, names);
    }

    [Test]
    public void Query_Single()
    {
        try
        {
            db.Author.Single();
            Assert.Fail();
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual("Set contains more than one element", ex.Message);
        }

        var sngPred = db.Author.Single(a => a.Name == "Alice");
        Assert.AreEqual("Alice", sngPred.Name);

        try
        {
            db.Author.Single(a => a.Name == "No name");
            Assert.Fail();
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual("Empty set", ex.Message);
        }
    }

    [Test]
    public void Query_SingleOrDefault()
    {
        try
        {
            db.Author.SingleOrDefault();
            Assert.Fail();
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual("Set contains more than one element", ex.Message);
        }

        var sngPred = db.Author.SingleOrDefault(a => a.Name == "Alice");
        Assert.AreEqual("Alice", sngPred?.Name);

        var sngNull = db.Author.SingleOrDefault(a => a.Name == "No name");
        Assert.AreEqual(null, sngNull);
    }

    [Test]
    public async Task Query_Skip()
    {
        var authors = await db.Author.Skip(1).ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Bob" }, names);
    }

    [Test]
    public void Query_Sum()
    {
        var sum1 = db.Author.Select(a => a.Age).Sum();
        Assert.AreEqual(58, sum1);

        var sum2 = db.Author.Sum(a => a.Age);
        Assert.AreEqual(58, sum2);

        var sum4 = db.Author.Sum(a => 1D * a.Age);
        Assert.AreEqual(58D, sum4);
    }

    [Test]
    public async Task Query_Take()
    {
        var authors = await db.Author.Take(1).ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Alice" }, names);
    }

    [Test]
    public async Task Query_Function_Value_Single()
    {
        var ret = await db.SayHello().SingleAsync();
        Assert.AreEqual("Hello!", ret);
    }

    [Test]
    public async Task Query_Function_Value_List()
    {
        var ret = await db.SayHello().ToListAsync();
        Assert.AreEqual(new List<string> { "Hello!" }, ret);
    }

    [Test]
    public async Task Query_Function_With_Param()
    {
        var ret = await db.Add2(8).SingleAsync();
        Assert.AreEqual(10, ret);
    }

    [Test]
    public async Task Query_Function_Array()
    {
        var ret = await db.SayHelloArray().ToListAsync();
        Assert.AreEqual(new List<string> { "Hello!", "Hello!" }, ret);
    }

    [Test]
    public async Task Query_Function_Set()
    {
        var ret = await db.GetAuthors().Select(x => x.Name).ToListAsync();
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, ret);
    }

    [Test]
    public async Task Query_CastTypes()
    {
        Assert.AreEqual(new[] { 91, 83 }, await db.GetAuthors().Select(a => (int)a.Score).ToArrayAsync());
        Assert.AreEqual(new[] { 32.0D, 26.0D }, await db.GetAuthors().Select(a => (double)a.Age).ToArrayAsync());
        Assert.AreEqual(new[] { 91L, 83L }, await db.GetAuthors().Select(a => (long)a.Score).ToArrayAsync());
    }
}
