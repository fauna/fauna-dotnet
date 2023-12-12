using NUnit.Framework;
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

        var expected = new QueryExpr(
            new QueryLiteral("Book.where(.title == "),
            new QueryVal<string>("foo"),
            new QueryLiteral(" && .genre == "),
            new QueryVal<string>("bar"),
            new QueryLiteral(")")
        );

        Assert.That(q, Is.EqualTo(expected));
    }

    [Test]
    public void BuildsAQueryWithSubquery()
    {
        int backorderLimit = 15;
        bool isBackordered = false;

        var subQuery = FQL($@"let product = Product.firstWhere(.backorderLimit == {backorderLimit} && .backordered == {isBackordered})!; product.quantity;");
        var actual = FQL($@"Product.where(.quantity == {subQuery}).order(.title) {{ name, description }}");

        var asd = new QueryLiteral("Product.where(.quantity == ");

        var expected = new QueryExpr(new QueryLiteral("Product.where(.quantity == "),
            new QueryExpr(
                new QueryLiteral("let product = Product.firstWhere(.backorderLimit == "),
                new QueryVal<int>(backorderLimit),
                new QueryLiteral(" && .backordered == "),
                new QueryVal<bool>(isBackordered),
                new QueryLiteral(")!; product.quantity;")
            ),
            new QueryLiteral(").order(.title) { name, description }")
        );

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void BuildsAQueryWithArrayParam()
    {
        var arrayParam = new object[] {
            "item1",
            143,
            false,
            new[] { "item21", "item22" }
        };

        var expected = new QueryExpr(new QueryLiteral("let x = "), new QueryVal<object[]>(arrayParam));

        var actual = FQL($@"let x = {arrayParam}");

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void BuildsAQueryWithListParam()
    {
        var listParam = new List<object> {
            "item1",
            143,
            false,
            new[] { "item21", "item22" }
        };

        var expected = new QueryExpr(new QueryLiteral("let x = "), new QueryVal<List<object>>(listParam));

        var actual = FQL($@"let x = {listParam}");

        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void BuildsAQueryWithDictionaryParam()
    {
        IDictionary<string, object> dictionaryParam = new Dictionary<string, object> {
            { "key1", "item1" },
            { "key2", 143 },
            { "key3", new[] { "item21", "item22" } }
        };

        var expected = new QueryExpr(new QueryLiteral("let x = "), new QueryVal<IDictionary<string, object>>(dictionaryParam));


        var actual = FQL($@"let x = {dictionaryParam}");

        Assert.That(actual, Is.EqualTo(expected));
    }
}
