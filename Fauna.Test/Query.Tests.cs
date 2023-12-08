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
        var listOfItems = new string[] { "item1", "item2", "item3" };

        var actual = FQL($@"let x = {listOfItems};");

        var queryArr = new QueryArr<string>(listOfItems);
        var expected = new QueryExpr(
            new QueryLiteral("let x = "),
            new QueryVal<QueryArr<string>>(queryArr),
            new QueryLiteral(";")
        );

        Assert.That(actual, Is.EqualTo(expected));
    }
}
