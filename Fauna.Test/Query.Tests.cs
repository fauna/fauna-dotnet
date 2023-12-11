using NUnit.Framework;
using System.Collections.Generic;
using static Fauna.Query;

namespace Fauna.Test;

[TestFixture]
public class QueryTests
{
    [Test]
    public void BuildsAQuery()
    {
        var p1 = "foo";
        var p2 = "bar";
        var q = FQL($@"Book.where(.title == {p1} && .genre == {p2})");

        var expected = new Query.Expr(
            "Book.where(.title == ",
            new Query.Val<string>("foo"),
            " && .genre == ",
            new Query.Val<string>("bar"),
            ")"
        );

        Assert.That(q, Is.EqualTo(expected));
    }

    [Test]
    public void BuildsAQuery_WithValidSubquery()
    {
        int backorderLimit = 15;
        bool isBackordered = false;

        var subQuery = FQL($@"let product = Product.firstWhere(.backorderLimit == {backorderLimit} && .backordered == {isBackordered})!; product.quantity;");
        var actual = FQL($@"Product.where(.quantity == {subQuery}).order(.title) {{ name, description }}");

        var expected = new Query.Expr("Product.where(.quantity == ",
            new Query.Expr(
                "let product = Product.firstWhere(.backorderLimit == ",
                new Query.Val<int>(backorderLimit),
                " && .backordered == ",
                new Query.Val<bool>(isBackordered),
                ")!; product.quantity;"
            ),
            ").order(.title) { name, description }"
        );

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void BuildsAQuery_WithValidArrayParam()
    {
        var arrayParam = new object[] {
            "item1",
            143,
            false,
            new[] { "item21", "item22" },
            new List<object> { "item1", 143, false, new[] { "item21", "item22" } },
            new Dictionary<string, object> {
                { "key1", "item1" },
                { "key2", 143 },
                { "key3", new[] { "item21", "item22" } }
            }
        };

        var expected = new Expr("let x = ", new Query.Val<object[]>(arrayParam));

        var actual = FQL($@"let x = {arrayParam}");

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void BuildsAQuery_WithValidListParam()
    {
        var listParam = new List<object> {
            "item1",
            143,
            false,
            new[] { "item21", "item22" },
            new List<object> { "item1", 143, false, new[] { "item21", "item22" } },
            new Dictionary<string, object> {
                { "key1", "item1" },
                { "key2", 143 },
                { "key3", new[] { "item21", "item22" } }
            }
        };

        var expected = new Expr("let x = ", new Query.Val<List<object>>(listParam));

        var actual = FQL($@"let x = {listParam}");

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void BuildsAQuery_WithValidDictionaryParam()
    {
        IDictionary<string, object> dictionaryParam = new Dictionary<string, object> {
            { "key1", "item1" },
            { "key2", 143 },
            { "key3", new[] { "item21", "item22" } },
            { "key4", new List<object> { "item1", 143, false, new[] { "item21", "item22" } } },
            { "key5", new Dictionary<string, object> {
                { "key1", "item1" },
                { "key2", 143 },
                { "key3", new[] { "item21", "item22" } }
            } }
        };

        var expected = new Expr("let x = ", new Query.Val<IDictionary<string, object>>(dictionaryParam));


        var actual = FQL($@"let x = {dictionaryParam}");

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void BuildsAQuery_WithInvalidNestedParam()
    {
        var expressionValue = FQL($@"2");
        var arrayParam = CreateNestedArray(5, expressionValue);
        var listParam = new List<object> { CreateNestedArray(5, expressionValue) };
        var dictionaryParam = CreateNestedDictionary(5, expressionValue);

        Assert.Throws<InvalidOperationException>(() => FQL($@"let x = {arrayParam}"));
        Assert.Throws<InvalidOperationException>(() => FQL($@"let x = {listParam}"));
        Assert.Throws<InvalidOperationException>(() => FQL($@"let x = {dictionaryParam}"));
    }

    private static Dictionary<string, object> CreateNestedDictionary(int depth, object innerValue)
    {
        object currentValue = innerValue;
        for (int i = depth - 1; i >= 0; i--)
        {
            currentValue = new Dictionary<string, object> { { $"level{i + 1}Key", currentValue } };
        }

        return new Dictionary<string, object> { { "level0Key", currentValue } };
    }

    private static object[] CreateNestedArray(int depth, object innerValue)
    {
        object currentValue = innerValue;
        for (int i = depth - 1; i >= 0; i--)
        {
            currentValue = new object[] { $"level{i + 1}Value", currentValue };
        }

        return ["level0Value", currentValue];
    }
}
