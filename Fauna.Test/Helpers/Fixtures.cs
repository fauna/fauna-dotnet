using Fauna.Mapping.Attributes;
using static Fauna.Query;

namespace Fauna.Test;

[Object]
public class Author
{
    [Field] public string? Id { get; set; }
    [Field] public string Name { get; set; } = "Alice";
    [Field] public int Age { get; set; }
}

public class AuthorDb : DataContext
{
    public class AuthorCol : Collection<Author>
    {
        public Index<Author> ByName(string name) => Index().Call(name);
    }

    public AuthorCol Author { get => GetCollection<AuthorCol>(); }

    public Function<int> Add2(int val) => Fn<int>().Call(val);
    public Function<string> SayHello() => Fn<string>("SayHello").Call();
    public Function<string> SayHelloArray() => Fn<string>("SayHelloArray").Call();
    public Function<Author> GetAuthors() => Fn<Author>().Call();

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
        client.QueryAsync(FQL($"Author.create({{name: 'Alice', age: 32 }})")).Wait();
        client.QueryAsync(FQL($"Author.create({{name: 'Bob', age: 26 }})")).Wait();
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

}
