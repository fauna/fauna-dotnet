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
    public void BuildsAQueryWithSubquery()
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
}
