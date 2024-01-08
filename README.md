# The Official .NET Driver for [Fauna](https://fauna.com/)

> [!CAUTION]
> This driver is currently under development and should not be used for production.

This driver can only be used with FQL v10, and is not compatible with earlier versions of FQL. To query your databases with earlier API versions, see [faunadb-csharp](https://github.com/fauna/faunadb-csharp).

See the [Fauna Documentation](https://docs.fauna.com/fauna/current/) for additional information about how to configure and query your databases.

## Compatibility

- C# ^10.0
- .NET 6.0
- .NET 7.0
- .NET 8.0


## Installation

TODO: Describe setup once package is in Nuget

## Basic Usage

```csharp
using Fauna.Exceptions;
using static Fauna.Query;

class Basics
{
     async Task Run()
    {
        var client = new Client("secret");
        const string hi = "Hello, Fauna!";
        var query = FQL($@"
        let x = {hi}
        x");

        try
        {
            var result = await client.QueryAsync<string>(query);
            Console.WriteLine(result.Data); // Hello, Fauna!
            Console.WriteLine(result.Stats.ToString()); // compute: 1, read: 0, write: 0, ...
        }
        catch (ServiceException e)
        {
            // Handle exceptions 
        }
    }
}
```

## Basic Usage with POCOs

Define serialization behavior with attributes on your POCOs.

```csharp
using Fauna.Serialization.Attributes;
using static Fauna.Query;

class BasicsWithPocos
{
    [FaunaObject]
    private class Person
    {
        [Field("first_name")]
        public string? FirstName { get; set; }
        
        [Field("last_name")]
        public string? LastName { get; set; }
        
        [Field("age", FaunaType.Long)]
        public int Age { get; set; }
    }
    
     async Task Run()
    {
        var client = new Client("secret");
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 42
        };
        
        // Single braces are for template variables, so escape them with double braces.
        var query = FQL($@"
        let x = {person}
        {{ first_name: x.first_name, last_name: x.last_name, age: x.age + 1}}");
  
        var result = await client.QueryAsync<Person>(query);
        Console.WriteLine(result.Data.FirstName); // John
        Console.WriteLine(result.Data.LastName); // Doe
        Console.WriteLine(result.Data.Age); // 43
        Console.WriteLine(result.Stats.ToString()); // compute: 1, read: 0, write: 0, ...
    }
}
```

## Basic Usage with Composition

Compose two or more FQL queries together.

```csharp
using static Fauna.Query;

class BasicsWithPocos
{
    
    async Task Run()
    {
        var client = new Client("secret");
        
        // Single braces are for template variables, so escape them with double braces.
        var getAge = FQL($@"
        let x = {{ age: 42 }}
        x.age");
        
        var isMinor = FQL($@"
        {getAge} < 18
        ");
  
        var result = await client.QueryAsync<bool>(isMinor);
        Console.WriteLine(result.Data); // False
        Console.WriteLine(result.Stats.ToString()); // compute: 1, read: 0, write: 0, ...
    }
}
```
