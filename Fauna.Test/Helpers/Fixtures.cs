using Fauna.Mapping.Attributes;
using Fauna.Types;
using static Fauna.Query;

namespace Fauna.Test;

[Object]
public class Author
{
    [Field] public string? Id { get; set; }
    [Field] public string Name { get; set; } = "Alice";
    [Field] public int Age { get; set; }
    [Field] public long Subscribers { get; set; }
    [Field] public double Score { get; set; }
}

[Object]
public class Book
{
    [Field] public string? Id { get; set; }
    [Field] public string Name { get; set; } = "War and Peace";
}

[Object]
public class StreamingSandbox
{
    [Field("foo")] public string? Foo { get; set; }
}


public class AuthorDb : DataContext
{
    public class AuthorCol : Collection<Author>
    {
        public Index<Author> ByName(string name) => Index().Call(name);
    }

    public class BookCol : Collection<Book>
    {
    }

    public AuthorCol Author { get => GetCollection<AuthorCol>(); }
    public BookCol Book { get => GetCollection<BookCol>(); }

    public async Task<int> Add2(int val) => await Fn<int>().CallAsync(val);
    public string SayHello() => Fn<string>("SayHello").Call();
    public List<string> SayHelloArray() => Fn<List<string>>("SayHelloArray").Call();
    public async Task<Page<Author>> GetAuthors() => await Fn<Page<Author>>().CallAsync();

}

public class EmbeddedSet
{
    [Field] public string? Id { get; set; }
    [Field] public int Num { get; set; }
}

public class EmbeddedSetDb : DataContext
{
    public class EmbeddedSetCol : Collection<EmbeddedSet>
    { }

    public EmbeddedSetCol OneHundredB { get => GetCollection<EmbeddedSetCol>(); }
}

public static class Fixtures
{
    public static AuthorDb AuthorDb(Client client)
    {
        client.QueryAsync(FQL($"Collection.byName('Author')?.delete()")).Wait();
        client.QueryAsync(FQL($"Collection.byName('Book')?.delete()")).Wait();
        client.QueryAsync(FQL($"Function.byName('SayHello')?.delete()")).Wait();
        client.QueryAsync(FQL($"Function.byName('SayHelloArray')?.delete()")).Wait();
        client.QueryAsync(FQL($"Function.byName('GetAuthors')?.delete()")).Wait();
        client.QueryAsync(FQL($"Function.byName('Add2')?.delete()")).Wait();

        client.QueryAsync(FQL(
            $@"Collection.create({{
                name: 'Author',
                indexes: {{
                    byName: {{
                        terms: [{{ field: '.name' }}]
                    }}
                }}
            }})"))
            .Wait();
        client.QueryAsync(FQL($"Author.create({{name: 'Alice', age: 32, subscribers: 10000000, score: 91.3 }})")).Wait();
        client.QueryAsync(FQL($"Author.create({{name: 'Bob', age: 26, subscribers: 300000000, score: 83.5 }})")).Wait();

        client.QueryAsync(FQL(
                $@"Collection.create({{
                name: 'Book'
            }})"))
            .Wait();

        client.QueryAsync(FQL($"Function.create({{name: 'SayHello', body: '() => \"Hello!\"'}})")).Wait();
        client.QueryAsync(FQL($"Function.create({{name: 'SayHelloArray', body: '() => [SayHello(), SayHello()]'}})")).Wait();
        client.QueryAsync(FQL($"Function.create({{name: 'GetAuthors', body: '() => Author.all()'}})")).Wait();
        client.QueryAsync(FQL($"Function.create({{name: 'Add2', body: '(t) => t + 2'}})")).Wait();
        return client.DataContext<AuthorDb>();
    }

    public static EmbeddedSetDb EmbeddedSetDb(Client client)
    {
        client.QueryAsync(FQL($"Collection.byName('EmbeddedSet')?.delete()")).Wait();
        client.QueryAsync(FQL(
                $"Collection.create({{name: 'EmbeddedSet'}})"))
            .Wait();
        client.QueryAsync(FQL($"Set.sequence(0,100).forEach(i => EmbeddedSet.create({{num: i}}))")).Wait();
        return client.DataContext<EmbeddedSetDb>();
    }

    public static async Task StreamingSandboxSetup(Client client)
    {
        await client.QueryAsync(FQL($"Collection.byName('StreamingSandbox')?.delete()"));
        await client.QueryAsync(FQL($"Collection.create({{name: 'StreamingSandbox'}})"));
    }

}
