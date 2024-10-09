using System.Diagnostics.CodeAnalysis;
using Fauna.Exceptions;
using Fauna.Linq;
using Fauna.Mapping;
using Fauna.Types;
using NUnit.Framework;
using static Fauna.Query;
using static Fauna.Test.Helpers.TestClientHelper;

namespace Fauna.Test.E2E;


public class E2ELinqTestDb : DataContext
{
    public class E2ELinqTestAuthor
    {
        [Id] public string? Id { get; set; }
        [AllowNull] public string Name { get; set; }
    }

    public class E2ELinqTestBook
    {
        [Id] public string? Id { get; set; }
        [AllowNull] public string Name { get; set; }
        [AllowNull] public Ref<E2ELinqTestAuthor> Author { get; set; }
    }


    [Name("E2ELinqTest_Author")]
    public class AuthorCol : Collection<E2ELinqTestAuthor> { }

    [Name("E2ELinqTest_Book")]
    public class BookCol : Collection<E2ELinqTestBook> { }

    public AuthorCol Author { get => GetCollection<AuthorCol>(); }

    public BookCol Book { get => GetCollection<BookCol>(); }
}

public class E2ELinqTest
{
    private static readonly Client s_client = GetLocalhostClient();
    private static readonly E2ELinqTestDb s_db = s_client.DataContext<E2ELinqTestDb>();

    [OneTimeSetUp]
    public void SetUp()
    {
        s_db.QueryAsync(FQL($"Collection.byName('E2ELinqTest_Author')?.delete()")).Wait();
        s_db.QueryAsync(FQL($"Collection.byName('E2ELinqTest_Book')?.delete()")).Wait();
        s_db.QueryAsync(FQL($"Collection.create({{name: 'E2ELinqTest_Author'}})")).Wait();
        s_db.QueryAsync(FQL($"Collection.create({{name: 'E2ELinqTest_Book'}})")).Wait();
        var res = s_db.QueryAsync<Ref<Dictionary<string, object>>>(FQL($"E2ELinqTest_Author.create({{name: 'Leo Tolstoy'}})")).Result;
        s_db.QueryAsync(FQL($"E2ELinqTest_Book.create({{name: 'War and Peace', author: {res.Data} }})")).Wait();
    }

    [Test]
    public async Task E2ELinq_ObtainRefWithoutProjection()
    {
        var res = await s_db.Book
            .Where(b => b.Name == "War and Peace")
            .SingleAsync();
        Assert.AreEqual("War and Peace", res.Name);
        Assert.IsNotNull(res.Author.Id);
        Assert.AreEqual(new Module("E2ELinqTest_Author"), res.Author.Collection);
        Assert.IsNull(res.Author.Exists);
        Assert.IsFalse(res.Author.IsLoaded);
    }

    [Test]
    public async Task E2ELinq_ProjectRefIntoDocument()
    {
        var res = await s_db.Book
            .Where(b => b.Name == "War and Peace")
            .Select(b => new { b.Name, Author = new { b.Author.Get().Name } })
            .SingleAsync();
        Assert.AreEqual("War and Peace", res.Name);
        Assert.AreEqual("Leo Tolstoy", res.Author.Name);
    }

    [Test]
    public void E2ELinq_ProjectDeletedRefThrows()
    {
        var aut = s_db.QueryAsync<Ref<Dictionary<string, object>>>(FQL($"E2ELinqTest_Author.create({{name: 'Mary Shelley'}})")).Result;
        s_db.QueryAsync(FQL($"E2ELinqTest_Book.create({{name: 'Frankenstein', author: {aut.Data} }})")).Wait();
        s_db.QueryAsync(FQL($"E2ELinqTest_Author.where(a => a.name == 'Mary Shelley').forEach(d => d.delete())")).Wait();

        var ex = Assert.Throws<NullDocumentException>(() => s_db.Book
            .Where(b => b.Name == "Frankenstein")
            .Select(b => new { b.Name, Author = new { b.Author.Get().Name } })
            .Single())!;

        Assert.IsNotNull(ex.Id);
        Assert.IsNull(ex.Name);
        Assert.AreEqual(new Module("E2ELinqTest_Author"), ex.Collection);
        Assert.AreEqual("not found", ex.Cause);
    }
}
