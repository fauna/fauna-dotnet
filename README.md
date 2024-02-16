# The Official .NET Driver for [Fauna](https://fauna.com/)

> [!CAUTION]
> This driver is currently under development and should not be used for production.

This driver can only be used with FQL v10, and is not compatible with earlier versions of FQL. To query your databases with earlier API versions, see [faunadb-csharp](https://github.com/fauna/faunadb-csharp).

See the [Fauna Documentation](https://docs.fauna.com/fauna/current/) for additional information about how to configure and query your databases.

## Features

- Injection-safe query composition with interpolated string templates
- POCO-based data mapping
- Async LINQ API for type-safe querying

## Compatibility

- C# ^10.0
- .NET 6.0
- .NET 7.0
- .NET 8.0

## Installation

TODO: Describe setup once package is in Nuget

## Basic usage

```csharp
using Fauna;
using Fauna.Exceptions;
using static Fauna.Query;

class Basics
{
    static async Task Main()
    {
        try
        {
            var client = new Client("secret");

            var hi = "Hello, Fauna!";

            // The FQL template function safely interpolates values. 
            var helloQuery = FQL($@"
                let x = {hi}
                x");

            // Optionally specify the expected result type as a type parameter.
            // If not provided, the value will be deserialized as object?
            var hello = await client.QueryAsync<string>(helloQuery);

            Console.WriteLine(hello.Data); // Hello, Fauna!
            Console.WriteLine(hello.Stats.ToString()); // compute: 1, read: 0, write: 0, ...

            var peopleQuery = FQL($@"Person.all() {{ first_name }}");

            // PaginateAsync returns an IAsyncEnumerable of pages
            var people = client.PaginateAsync<Dictionary<string, object?>>(peopleQuery);

            await foreach (var page in people)
            {
                foreach (var person in page.Data)
                {
                    Console.WriteLine($"Hello, {person["first_name"]}!"); // Hello, John! ...
                }
            }
        }
        catch (FaunaException e)
        {
            // Handle exceptions 
        }
    }
}
```

## Writing more complex queries

The FQL template DSL supports arbitrary composition of subqueries along with values.

```csharp
var client = new Client("secret");
        
var predicate = args[0] switch {
    "first" => FQL($".first_name == {args[1]}"),
    "last"  => FQL($".last_name" == {args[1]}),
    _       => throw ArgumentException(),
};
        
// Single braces are for template variables, so escape them with double braces.
var getPerson = FQL($"Person.firstWhere({predicate}) {{ id, first_name, last_name }}");
        
// Documents can be mapped to Dictionaries as well as POCOs (see below)
var result = await client.QueryAsync<Dictionary<string, object?>>(getPerson);

Console.WriteLine(result.Data["id"]);
Console.WriteLine(result.Data["first_name"]);
Console.WriteLine(result.Data["last_name"]);
```

## Database contexts and POCO data mapping

Fauna.Mapping.Attributes and the Fauna.DataContext class provide the ability to bring your Fauna database schema into your code.

### POCO Mapping

You can use attributes to map a POCO class to a Fauna document or object shape:

```csharp
using Fauna.Mapping.Attributes;

[Object]
class Person
{
    // Property names are automatically converted to camelCase.
    [Field]
    public string? Id { get; set; }

    // Manually specify a name by providing a string.
    [Field("first_name")]
    public string? FirstName { get; set; }

    [Field("last_name")]
    public string? LastName { get; set; }

    [Field]
    public int Age { get; set; }
}
```

Your POCO classes can be used to drive deserialization:

```csharp
var peopleQuery = FQL($@"Person.all()");
var people = client.PaginateAsync<Person>(peopleQuery).FlattenAsync();

await foreach (var p in people)
{
    Console.WriteLine($"{p.FirstName} {p.LastName}");
}
```

As well as to write to your database:

```csharp
var person = new Person { FirstName = "John", LastName = "Smith", Age = 42 };

var result = await client.QueryAsync($@"Person.create({person}).id");
Console.WriteLine(result.Data); // 69219723210223...
```

### DataContext

The DataContext class provides a schema-aware view of your database. Subclass it and configure your collections: 

```csharp
class PersonDb : DataContext
{
    public class PersonCollection : Collection<Person>
    {
        public Index<Person> ByFirstName(string first) => Index().Call(first);
        public Index<Person> ByLastName(string last) => Index().Call(last);
    }

    public PersonCollection Person { get => GetCollection<PersonCollection>(); }
}
```

DataContext provides Client querying which automatically maps your collections' documents to their POCO equivalents even when type hints are not provided.

```csharp
var db = client.DataContext<PersonDb>

var result = db.QueryAsync($"Person.all().first()");
var person = (Person)result.Data!;

Console.WriteLine(person.FirstName);
```

### LINQ-based queries

Last but not least, your DataContext subclass provides a LINQ-compatible API for type-safe querying.

```csharp
// general query
db.Person.Where(p => p.FirstName == "John")
         .Select(p => new { p.FirstName, p.LastName })
         .First();

// or start with an index
db.Person.ByFirstName("John")
         .Select(p => new { p.FirstName, p.LastName })
         .First();
```

There are async variants of methods which execute queries:

```csharp
var syncCount = db.Person.Count();
var asyncCount = await db.Person.CountAsync();
```

## Paginating [Fauna Sets](https://docs.fauna.com/fauna/current/reference/reference/schema_entities/set/)

When you wish to paginate a Set, such as a Collection or Index, use `Client.Paginate`.

Example of a query that returns a Set:
```csharp
var query = FQL($"Person.all()");
await foreach (var page in client.PaginateAsync<Person>(query))
{
    // handle each page
}

await foreach (var item in client.PaginateAsync<Person>(query).FlattenAsync())
{
    // handle each item
}
```

Example of a query that returns an object with an embedded Set:
```csharp
[Object]
class MyResult
{
    [Field("users")]
    public Page<Person>? Users { get; set; }
}

var query = FQL($"{{users: Person.all()}}");
var result = await client.QueryAsync<MyResult>(query);

await foreach (var page in client.PaginateAsync(result.Data.Users!))
{
    // handle each page
} 

await foreach (var item in client.PaginateAsync(result.Data.Users!).FlattenAsync())
{
    // handle each item
} 
```
