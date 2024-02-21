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
