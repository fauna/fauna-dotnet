using NUnit.Framework;
using static Fauna.Query;
using System.Linq.Expressions;

namespace Fauna.Test;

// POCO
public abstract class Document
{
    public string ID { get; set; }
}

public class Author : Document
{
    public static string Bar = "Bar";
    public string Name { get; set; }
    public int Age { get; set; }
    public string Foo() => "Foo";
}

public class DB
{
    public static LinqCollection<Author> Author() => new LinqCollection<Author>("Author");
}

[TestFixture]
public class LinqTests
{
    [Test]
    public void BuildsAQuery()
    {
        // // a basic query
        // var q1 = from author in DB.Author()
        //          where author.Name == "Alice"
        //          select author;

        // var expr1 = ((IQueryable)q1).Expression;
        // Console.WriteLine(expr1);
        // var fql1 = LinqQueryBuilder.Build(expr1);
        // Console.WriteLine($"FQL!! {fql1}")m

        // Outside values:
        // - free variables in expr (part of C# closure)
        // - static members

        // query referring to closed over variable
        string q2Name = "Alice";
        string q2Name2 = "Bob";
        var q2 = from author in DB.Author()
                 where author.Name == q2Name
                 where author.Name == q2Name2
                 where author.Name == Author.Bar
                 where author.Age == q2Name.Length
                 select author;

        var expr2 = ((IQueryable)q2).Expression;
        Console.WriteLine($"EXPR {expr2}");
        var fql2 = LinqQueryBuilder.Build(expr2);
        Console.WriteLine($"FQL!! {fql2}");
    }
}
