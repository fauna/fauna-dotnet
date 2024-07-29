using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using static Fauna.Test.Helpers.TestClientHelper;

namespace Fauna.Test.Linq;

[TestFixture]
public class QueryTests
{

    [AllowNull]
    private static Client _client = null!;
    [AllowNull]
    private static AuthorDb _db = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        _client = NewTestClient();
        _db = Fixtures.AuthorDb(_client);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    [Test]
    public async Task Collection_PaginateAsync()
    {
        var names = new List<string>();
        await foreach (var p in _db.Author.PaginateAsync())
        {
            names.AddRange(p.Data.Select(a => a.Name));
        }
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }


    [Test]
    public async Task Index_PaginateAsync()
    {
        var names = new List<string>();
        await foreach (var p in _db.Author.ByName("Alice").PaginateAsync())
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
            await foreach (var p in _db.Author.PaginateAsync(cancel: c))
            {
                Console.WriteLine(p.Data);
            }
        });
        await TestCancel("ToAsyncEnumerable", async (c) =>
        {
            await foreach (var a in _db.Author.ToAsyncEnumerable(cancel: c))
            {
                Console.WriteLine(a);
            }
        });

        await TestCancel("AllAsync", async (c) => await _db.Author.AllAsync(d => true, c));
        await TestCancel("AnyAsync", async (c) => await _db.Author.AnyAsync(c));
        await TestCancel("AnyAsync", async (c) => await _db.Author.AnyAsync(d => true, c));
        await TestCancel("CountAsync", async (c) => await _db.Author.CountAsync(c));
        await TestCancel("CountAsync", async (c) => await _db.Author.CountAsync(d => true, c));
        await TestCancel("FirstAsync", async (c) => await _db.Author.FirstAsync(c));
        await TestCancel("FirstAsync", async (c) => await _db.Author.FirstAsync(d => true, c));
        await TestCancel("FirstOrDefaultAsync", async (c) => await _db.Author.FirstOrDefaultAsync(c));
        await TestCancel("FirstOrDefaultAsync", async (c) => await _db.Author.FirstOrDefaultAsync(d => true, c));
        await TestCancel("LastAsync", async (c) => await _db.Author.LastAsync(c));
        await TestCancel("LastAsync", async (c) => await _db.Author.LastAsync(d => true, c));
        await TestCancel("LastOrDefaultAsync", async (c) => await _db.Author.LastOrDefaultAsync(c));
        await TestCancel("LastOrDefaultAsync", async (c) => await _db.Author.LastOrDefaultAsync(d => true, c));
        await TestCancel("LongCountAsync", async (c) => await _db.Author.LongCountAsync(c));
        await TestCancel("LongCountAsync", async (c) => await _db.Author.LongCountAsync(d => true, c));
        await TestCancel("MaxAsync", async (c) => await _db.Author.MaxAsync(c));
        await TestCancel("MaxAsync", async (c) => await _db.Author.MaxAsync(d => true, c));
        await TestCancel("MinAsync", async (c) => await _db.Author.MinAsync(c));
        await TestCancel("MinAsync", async (c) => await _db.Author.MinAsync(d => true, c));
        await TestCancel("SumAsync", async (c) => await _db.Author.SumAsync(d => (int)1, c));
        await TestCancel("SumAsync", async (c) => await _db.Author.SumAsync(d => (long)1L, c));
        await TestCancel("SumAsync", async (c) => await _db.Author.SumAsync(d => (double)1D, c));
        await TestCancel("AverageAsync", async (c) => await _db.Author.AverageAsync(d => (int)1, c));
        await TestCancel("AverageAsync", async (c) => await _db.Author.AverageAsync(d => (long)1L, c));
        await TestCancel("AverageAsync", async (c) => await _db.Author.AverageAsync(d => (double)1D, c));

        await TestCancel("ToListAsync", async (c) => await _db.Author.ToListAsync(c));
        await TestCancel("ToArrayAsync", async (c) => await _db.Author.ToArrayAsync(c));
        await TestCancel("ToHashSetAsync", async (c) => await _db.Author.ToHashSetAsync(c));
        await TestCancel("ToDictionaryAsync", async (c) =>
            await _db.Author.ToDictionaryAsync(a => a.Name, a => a.Age, c));
    }

    [Test]
    public async Task Query_ToAsyncEnumerable()
    {
        var names = new List<string>();
        await foreach (var a in _db.Author.ToAsyncEnumerable())
        {
            names.Add(a.Name);
        }
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }

    [Test]
    public void Query_ToEnumerable()
    {
        var names = new List<string>();
        foreach (var a in _db.Author.ToEnumerable())
        {
            names.Add(a.Name);
        }
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }

    [Test]
    public async Task Query_ToListAsync()
    {
        var names = await _db.Author.Select(a => a.Name).ToListAsync();
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }

    [Test]
    public void Query_ToList()
    {
        var names = _db.Author.Select(a => a.Name).ToList();
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }

    [Test]
    public async Task Query_ToArrayAsync()
    {
        var names = await _db.Author.Select(a => a.Name).ToArrayAsync();
        Assert.AreEqual(new string[] { "Alice", "Bob" }, names);
    }

    [Test]
    public void Query_ToArray()
    {
        var names = _db.Author.Select(a => a.Name).ToArray();
        Assert.AreEqual(new string[] { "Alice", "Bob" }, names);
    }

    [Test]
    public async Task Query_ToHashSetAsync()
    {
        var names = await _db.Author.Select(a => a.Name).ToHashSetAsync();
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }

    [Test]
    public void Query_ToHashSet()
    {
        var names = _db.Author.Select(a => a.Name).ToHashSet();
        Assert.AreEqual(new HashSet<string> { "Alice", "Bob" }, names);
    }

    [Test]
    public async Task Query_ToDictionaryAsyncSelector()
    {
        var dict = await _db.Author.ToDictionaryAsync(a => a.Name, a => a.Age);
        Assert.AreEqual(new Dictionary<string, int> { { "Alice", 32 }, { "Bob", 26 } }, dict);
    }

    [Test]
    public void Query_ToDictionarySelector()
    {
        var dict = _db.Author.ToDictionary(a => a.Name, a => a.Age);
        Assert.AreEqual(new Dictionary<string, int> { { "Alice", 32 }, { "Bob", 26 } }, dict);
    }

    [Test]
    public async Task Query_ToDictionaryAsync()
    {
        var dict = await _db.Author.Select(a => ValueTuple.Create(a.Name, a.Age)).ToDictionaryAsync();
        Assert.AreEqual(new Dictionary<string, int> { { "Alice", 32 }, { "Bob", 26 } }, dict);
    }

    [Test]
    public void Query_ToDictionary()
    {
        var dict = _db.Author.Select(a => ValueTuple.Create(a.Name, a.Age)).ToDictionary();
        Assert.AreEqual(new Dictionary<string, int> { { "Alice", 32 }, { "Bob", 26 } }, dict);
    }

    [Test]
    public async Task Query_ExpressionSyntax()
    {
        var q1 = from a in _db.Author
                 where a.Name == "Alice"
                 select a.Age;

        Assert.AreEqual(new List<int> { 32 }, await q1.ToListAsync());

        var q2 = from a in _db.Author.ByName("Alice")
                 select a.Age;

        Assert.AreEqual(new List<int> { 32 }, await q2.ToListAsync());

        var q3 = from a in _db.Author
                 orderby a.Age
                 select a.Name;

        Assert.AreEqual(new List<string> { "Bob", "Alice" }, await q3.ToListAsync());
    }

    [Test]
    public async Task Query_Where()
    {
        var authors = await _db.Author.Where(a => a.Name == "Alice").ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Alice" }, names);
    }

    [Test]
    public async Task Query_WhereQuery_ClosureRef()
    {
        var name = "Alice";
        var authors = await _db.Author.Where(a => a.Name == name).ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Alice" }, names);
    }

    static readonly string _aliceName = "Alice";

    [Test]
    public async Task Query_WhereQuery_ConstantField()
    {
        var authors = await _db.Author.Where(a => a.Name == _aliceName).ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Alice" }, names);
    }

    static string AliceName { get; } = "Alice";

    [Test]
    public async Task Query_WhereQuery_ConstantProp()
    {
        var authors = await _db.Author.Where(a => a.Name == AliceName).ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Alice" }, names);
    }

    static string AliceNameVirt { get => "Alice"; }

    [Test]
    public async Task Query_WhereQuery_ConstantVirtualProp()
    {
        var authors = await _db.Author.Where(a => a.Name == AliceNameVirt).ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Alice" }, names);
    }

    [Test]
    public async Task Query_Select_Field()
    {
        var names = await _db.Author.Select(a => a.Name).ToListAsync();
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }

    [Test]
    public async Task Query_Select_Projected()
    {
        var obj = await _db.Author.Select(a => new { a.Name }).ToListAsync();
        var names = obj.Select(o => o.Name);
        Assert.AreEqual(new List<string> { "Alice", "Bob", }, names);
    }

    private (string, string) Escaper(Author a) =>
        (a.Name, new String(a.Name.Reverse().ToArray()));

    [Test]
    public async Task Query_Select_Escaped()
    {
        var tups = await _db.Author.Select(a => Escaper(a)).ToListAsync();
        Assert.AreEqual(new List<(string, string)> { ("Alice", "ecilA"), ("Bob", "boB") }, tups);
    }

    [Test]
    public void Query_Any()
    {
        var any = _db.Author.Any();
        Assert.AreEqual(true, any);

        var anyPred = _db.Author.Any(a => a.Name == "Bob");
        Assert.AreEqual(true, anyPred);
    }

    [Test]
    public void Query_All()
    {
        var all = _db.Author.All(a => a.Name == "Bob");
        Assert.AreEqual(false, all);
    }

    [Test]
    public void Query_Count()
    {
        var count = _db.Author.Count();
        Assert.AreEqual(2, count);
    }

    [Test]
    public async Task Query_Distinct()
    {
        var ages = await _db.Author.Select(a => a.Age).Distinct().ToListAsync();
        Assert.AreEqual(new List<int> { 26, 32 }, ages);
    }

    [Test]
    public void Query_First()
    {
        var fst = _db.Author.First();
        Assert.AreEqual("Alice", fst.Name);

        var fstPred = _db.Author.First(a => a.Name == "Bob");
        Assert.AreEqual("Bob", fstPred.Name);

        try
        {
            _db.Author.First(a => a.Name == "No name");
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
        var fst = _db.Author.FirstOrDefault();
        Assert.AreEqual("Alice", fst?.Name);

        var fstPred = _db.Author.FirstOrDefault(a => a.Name == "Bob");
        Assert.AreEqual("Bob", fstPred?.Name);

        var fstNull = _db.Author.FirstOrDefault(a => a.Name == "No name");
        Assert.AreEqual(null, fstNull);
    }

    [Test]
    public void Query_Last()
    {
        var lst = _db.Author.Last();
        Assert.AreEqual("Bob", lst.Name);

        var lstPred = _db.Author.Last(a => a.Name == "Alice");
        Assert.AreEqual("Alice", lstPred.Name);

        try
        {
            _db.Author.Last(a => a.Name == "No name");
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
        var lst = _db.Author.LastOrDefault();
        Assert.AreEqual("Bob", lst?.Name);

        var lstPred = _db.Author.LastOrDefault(a => a.Name == "Alice");
        Assert.AreEqual("Alice", lstPred?.Name);

        var lstNull = _db.Author.LastOrDefault(a => a.Name == "No name");
        Assert.AreEqual(null, lstNull);
    }

    [Test]
    public void Query_LongCount()
    {
        var count = _db.Author.LongCount();
        Assert.AreEqual(2L, count);
    }

    [Test]
    public void Query_Max()
    {
        var max1 = _db.Author.Select(a => a.Age).Max();
        Assert.AreEqual(32, max1);

        var max2 = _db.Author.Max(a => a.Age);
        Assert.AreEqual(32, max2);

        try
        {
            _db.Author.Where(a => a.Name == "No name").Max(a => a.Age);
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
        var min1 = _db.Author.Select(a => a.Age).Min();
        Assert.AreEqual(26, min1);

        var min2 = _db.Author.Min(a => a.Age);
        Assert.AreEqual(26, min2);

        try
        {
            _db.Author.Where(a => a.Name == "No name").Min(a => a.Age);
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
        var avg = _db.Author.Average(a => a.Score);
        Assert.IsInstanceOf(typeof(double), avg);
        Assert.AreEqual(87.400000000000006, avg);

        var avgCastInt = _db.Author.Average(a => a.Age);
        Assert.IsInstanceOf(typeof(double), avgCastInt);
        Assert.AreEqual(29.0D, avgCastInt);

        var avgCastLong = _db.Author.Average(a => a.Subscribers);
        Assert.IsInstanceOf(typeof(double), avgCastLong);
        Assert.AreEqual(155000000.0D, avgCastLong);

        Assert.AreEqual(29.0D, _db.Author.AverageAsync(a => a.Age).Result);

        Assert.Throws<InvalidOperationException>(() => _db.Author.Where(a => a.Name == "No name").Average(a => a.Age), "Empty set");
    }

    [Test]
    public async Task Query_Order()
    {
        var names = new List<string>();
        await foreach (var a in _db.Author.Reverse().Order().ToAsyncEnumerable())
        {
            names.Add(a.Name);
        }
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, names);
    }

    [Test]
    public async Task Query_OrderBy()
    {
        var authors = await _db.Author.OrderBy(a => a.Age).ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Bob", "Alice" }, names);
    }

    [Test]
    public async Task Query_OrderDescending()
    {
        var authors = await _db.Author.OrderDescending().ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Bob", "Alice" }, names);
    }

    [Test]
    public async Task Query_OrderByDescending()
    {
        var authors = await _db.Author.OrderByDescending(a => a.Name).ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Bob", "Alice" }, names);
    }

    [Test]
    public async Task Query_Reverse()
    {
        var authors = await _db.Author.Reverse().ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Bob", "Alice" }, names);
    }

    [Test]
    public void Query_Single()
    {
        try
        {
            _db.Author.Single();
            Assert.Fail();
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual("Set contains more than one element", ex.Message);
        }

        var sngPred = _db.Author.Single(a => a.Name == "Alice");
        Assert.AreEqual("Alice", sngPred.Name);

        try
        {
            _db.Author.Single(a => a.Name == "No name");
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
            _db.Author.SingleOrDefault();
            Assert.Fail();
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual("Set contains more than one element", ex.Message);
        }

        var sngPred = _db.Author.SingleOrDefault(a => a.Name == "Alice");
        Assert.AreEqual("Alice", sngPred?.Name);

        var sngNull = _db.Author.SingleOrDefault(a => a.Name == "No name");
        Assert.AreEqual(null, sngNull);
    }

    [Test]
    public async Task Query_Skip()
    {
        var authors = await _db.Author.Skip(1).ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Bob" }, names);
    }

    [Test]
    public void Query_Sum()
    {
        var sum1 = _db.Author.Select(a => a.Age).Sum();
        Assert.AreEqual(58, sum1);

        var sum2 = _db.Author.Sum(a => a.Age);
        Assert.AreEqual(58, sum2);

        var sum4 = _db.Author.Sum(a => 1D * a.Age);
        Assert.AreEqual(58D, sum4);
    }

    [Test]
    public async Task Query_Take()
    {
        var authors = await _db.Author.Take(1).ToListAsync();
        var names = authors.Select(a => a.Name);
        Assert.AreEqual(new List<string> { "Alice" }, names);
    }

    [Test]
    public void Query_Function_Value_Single()
    {
        var ret = _db.SayHello();
        Assert.AreEqual("Hello!", ret);
    }

    [Test]
    public async Task Query_Function_With_Param()
    {
        var ret = await _db.Add2(8);
        Assert.AreEqual(10, ret);
    }

    [Test]
    public void Query_Function_Array()
    {
        var ret = _db.SayHelloArray();
        Assert.AreEqual(new List<string> { "Hello!", "Hello!" }, ret);
    }

    [Test]
    public async Task Query_Function_Set()
    {
        var ret = await _db.GetAuthors();
        Assert.AreEqual(new List<string> { "Alice", "Bob" }, ret.Data.Select(a => a.Name));
    }

    [Test]
    public async Task Query_CastTypes()
    {
        Assert.AreEqual(new[] { 91, 83 }, await _db.Author.Select(a => (int)a.Score).ToArrayAsync());
        Assert.AreEqual(new[] { 32.0D, 26.0D }, await _db.Author.Select(a => (double)a.Age).ToArrayAsync());
        Assert.AreEqual(new[] { 91L, 83L }, await _db.Author.Select(a => (long)a.Score).ToArrayAsync());
    }
}
