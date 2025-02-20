# Official .NET Driver for [Fauna v10](https://fauna.com/) (current)

This driver can only be used with FQL v10, and is not compatible with earlier versions of FQL. To query your databases with earlier API versions, see [faunadb-csharp](https://github.com/fauna/faunadb-csharp).

See the [Fauna Documentation](https://docs.fauna.com/fauna/current/) for additional information about how to configure and query your databases.

## Features

- Injection-safe query composition with interpolated string templates
- POCO-based data mapping
- Async LINQ API for type-safe querying

## Compatibility

- C# ^10.0
- .NET 8.0

## Installation

Using the .NET CLI:

```
dotnet add package Fauna
```

## API reference

API reference documentation for the driver is available at
https://fauna.github.io/fauna-dotnet/. The docs are generated using
[Doxygen](https://www.doxygen.nl/).


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
            // The client's authentication secret
            // defaults to the `FAUNA_SECRET` env var.
            var client = new Client();

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
var client = new Client();

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

* `[Id]`: Should only be used once per class  on a field that represents the Fauna document ID. It's not encoded unless the isClientGenerated flag is true.
* `[Ts]`: Should only be used once per class on a field that represents the timestamp of a document. It's not encoded.
* `[Collection]`: Typically goes unmodeled. Should only be used once per class on a field that represents the collection field of a document. It will never be encoded.
* `[Field]`: Can be associated with any field to override its name in Fauna.
* `[Ignore]`: Can be used to ignore fields during encoding and decoding.

```csharp
using Fauna.Mapping;

class Person
{
    // Property names are automatically converted to camelCase.
    [Id]
    public string? Id { get; set; }

    // Manually specify a name by providing a string.
    [Field("first_name")]
    public string? FirstName { get; set; }

    [Field("last_name")]
    public string? LastName { get; set; }

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
    public int AddTwo(int val) => Fn<int>().Call(val);
    public async Task<int> TimesTwo(int val) => await Fn<int>("MultiplyByTwo").CallAsync(val);
}
```

DataContext provides Client querying which automatically maps your collections' documents to their POCO equivalents even when type hints are not provided.

```csharp
var db = client.DataContext<PersonDb>

var result = db.QueryAsync($"Person.all().first()");
var person = (Person)result.Data!;

Console.WriteLine(person.FirstName);
```

### LINQ-based queries (preview)

> [!IMPORTANT]
> This functionality is in preview and may change in future releases.

DataContext provides a LINQ-compatible API for type-safe querying.

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

When you wish to paginate a Set, such as a Collection or Index, use `PaginateAsync`.

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

## Null Documents
A null document ([NullDoc](https://docs.fauna.com/fauna/current/reference/fql_reference/types#nulldoc)) can be handled two ways.

Option 1, you can let the driver throw an exception and do something with it.
```csharp
try {
    await client.QueryAsync<SomeCollDoc>(FQL($"SomeColl.byId('123')"))
} catch (NullDocumentException e) {
    Console.WriteLine(e.Id); // "123"
    Console.WriteLine(e.Collection.Name); // "SomeColl"
    Console.WriteLine(e.Cause); // "not found"
}
```

Option 2, you wrap your expected type in a Ref<> or NamedRef<>. Supported types are Dictionary<string,object> and POCOs.
```csharp
var q = FQL($"Collection.byName('Fake')");
var r = (await client.QueryAsync<NamedRef<Dictionary<string,object>>>(q)).Data;
if (r.Data.Exists) {
    Console.WriteLine(d.Id); // "Fake"
    Console.WriteLine(d.Collection.Name); // "Collection"
    var doc = r.Get(); // A dictionary with id, coll, ts, and any user-defined fields.
} else {
    Console.WriteLine(d.Name); // "Fake"
    Console.WriteLine(d.Collection.Name); // "Collection"
    Console.WriteLine(d.Cause); // "not found"
    r.Get() // this throws a NullDocumentException
}

```

## Event feeds (beta)

The driver supports [event
feeds](https://docs.fauna.com/fauna/current/learn/cdc/#event-feeds).

An event feed asynchronously polls an [event
source](https://docs.fauna.com/fauna/current/learn/cdc/#create-an-event-source)
for events.

To get paginated events, pass an [event
source](https://docs.fauna.com/fauna/current/learn/cdc/#create-an-event-source)
or a query that produces an event source to `EventFeedAsync()`:

```csharp
// Get an event source from a supported Set
EventSource eventSource = await client.QueryAsync<EventSource>(FQL($"Person.all().eventSource()"));

// Calculate timestamp for 10 minutes ago in microseconds
long tenMinutesAgo = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeMilliseconds() * 1000;
var feedOptions = new FeedOptions(startTs: tenMinutesAgo, pageSize: 10);

// Pass the event source and `FeedOptions` to `EventFeedAsync()`:
var feed = await client.EventFeedAsync<Person>(eventSource, feedOptions);

// You can also pass a query that produces an event source directly to `EventFeedAsync()`:
var feedFromQuery = await client.EventFeedAsync<Person>(FQL($"Person.all().eventsOn({{ .price, .stock }})"), feedOptions);

// EventFeedAsync() returns a `FeedEnumerable` instance that can act as an `AsyncEnumerator`.
// Use `foreach()` to iterate through the pages of events.
await foreach (var page in feed)
{
    foreach (var evt in page.Events)
    {
        Console.WriteLine($"Event Type: {evt.Type}");
        Person person = evt.Data;
        Console.WriteLine($"First Name: {person.FirstName} - Last Name: {person.LastName} - Age: {person.Age}");
    }
}
```

## Event streams

The driver supports [event streams](https://docs.fauna.com/fauna/current/learn/cdc/#event-streaming).

To start and subscribe to an event stream, pass a query that produces an [event
source](https://docs.fauna.com/fauna/current/learn/cdc/#create-an-event-source)
to `EventStreamAsync()`:

```csharp
var stream = await client.EventStreamAsync<Person>(FQL($"Person.all().eventSource()"));
await foreach (var evt in stream)
{
    Console.WriteLine($"Received Event Type: {evt.Type}");
    if (evt.Data != null) // Status events won't have Data
    {
        Person person = evt.Data;
        Console.WriteLine($"First Name: {person.FirstName} - Last Name: {person.LastName} - Age: {person.Age}");
    }
}
```

## Debug logging

To enable debug logging, set the `FAUNA_DEBUG` environment variable to an integer for the `Microsoft.Extensions.Logging.LogLevel`. For example:

* `0`: `LogLevel.Trace` and higher (all messages)
* `3`: `LogLevel.Warning` and higher

The driver logs HTTP request and response details, including headers. For security, the `Authorization` header is redacted in debug logs but is visible in trace logs.

> [!NOTE]
> As of v1.0.0, the driver only outputs `LogLevel.Debug` messages. Use `0` (Trace) or `1` (Debug) to log these messages.

For advanced logging, you can use a custom `ILogger` implementation, such as Serilog or NLog. Pass the implementation to the `Configuration` class when instantiating a `Client`.

### Basic example: Serilog

Install the packages:
```
$ dotnet add package Serilog
$ dotnet add package Serilog.Extensions.Logging
$ dotnet add package Serilog.Sinks.Console
$ dotnet add package Serilog.Sinks.File
```

Configure and use the logger:
```csharp
using Fauna;
using Microsoft.Extensions.Logging;
using Serilog;
using static Fauna.Query;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .WriteTo.File("log.txt",
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true)
    .CreateLogger();

var logFactory = new LoggerFactory().AddSerilog(Log.Logger);

var config = new Configuration("mysecret", logger: logFactory.CreateLogger("myapp"));

var client = new Client(config);

await client.QueryAsync(FQL($"1+1"));

// You should see LogLevel.Debug messages in both the Console and the "log{date}.txt" file
```
