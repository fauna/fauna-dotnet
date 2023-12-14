using NUnit.Framework;
using System.Text.RegularExpressions;
using static Fauna.Query;

namespace Fauna.Test;

[TestFixture]
public class QueryExprTests
{
    [Test]
    public void Ctor_WithList()
    {
        var fragment1 = new QueryLiteral("fragment1");
        var fragment2 = new QueryExpr(new QueryLiteral("fragment2"));
        var fragment3 = new QueryVal<int>(123);
        var expected = new List<IQueryFragment> { fragment1, fragment2, fragment3 };

        var actual1 = new QueryExpr(expected);
        var actual2 = new QueryExpr(fragment1, fragment2, fragment3);

        Assert.AreEqual(expected.AsReadOnly(), actual1.Fragments);
        Assert.AreEqual(expected.AsReadOnly(), actual2.Fragments);
    }

    [Test]
    public void Ctor_WithValidFragments_ShouldNotThrowException()
    {
        var validFragments = new IQueryFragment[]
        {
        new QueryLiteral("fragment1"),
        new QueryExpr(new QueryLiteral("fragment2")),
        new QueryVal<int>(123)
        };

        Assert.DoesNotThrow(() => new QueryExpr(validFragments));
    }

    [Test]
    public void ToString_ReturnsExpectedValue()
    {
        var expr = new QueryExpr(new QueryLiteral("item1"), new QueryLiteral("item2"));
        var expectedString = "QueryExpr(QueryLiteral(item1),QueryLiteral(item2))";
        Assert.AreEqual(expectedString, expr.ToString());
    }

    [Test]
    public void GetHashCode_ReturnsConsistentValue()
    {
        var expr = new QueryExpr(new QueryLiteral("item1"));
        int hashCode1 = expr.GetHashCode();
        int hashCode2 = expr.GetHashCode();
        Assert.AreEqual(hashCode1, hashCode2);
    }

    [Test]
    public void Equality_WithSameValue()
    {
        var expr1 = new QueryExpr(new QueryLiteral("item1"), new QueryLiteral("item2"));
        var expr2 = new QueryExpr(new QueryLiteral("item1"), new QueryLiteral("item2"));
        Assert.IsTrue(expr1.Equals(expr2));
    }

    [Test]
    public void Inequality_WithDifferentValue()
    {
        var expr1 = new QueryExpr(new QueryLiteral("item1"), new QueryLiteral("item2"));
        var expr2 = new QueryExpr(new QueryLiteral("item3"), new QueryLiteral("item4"));
        Assert.IsFalse(expr1.Equals(expr2));
    }

    [Test]
    public void EqualityOperator_WithSameValue()
    {
        var expr1 = new QueryExpr(new QueryLiteral("item1"), new QueryLiteral("item2"));
        var expr2 = new QueryExpr(new QueryLiteral("item1"), new QueryLiteral("item2"));
        Assert.IsTrue(expr1 == expr2);
    }

    [Test]
    public void InequalityOperator_WithDifferentValue()
    {
        var expr1 = new QueryExpr(new QueryLiteral("item1"), new QueryLiteral("item2"));
        var expr2 = new QueryExpr(new QueryLiteral("item3"), new QueryLiteral("item4"));
        Assert.IsTrue(expr1 != expr2);
    }

    [Test]
    public void Serialize_WithSubquery()
    {
        int backorderLimit = 15;
        bool isBackordered = false;
        var subQuery = FQL($@"let product = Product.firstWhere(.backorderLimit == {backorderLimit} && .backordered == {isBackordered})!; product.quantity;");
        var mainQuery = FQL($@"Product.where(.quantity == {subQuery}).order(.title) {{ name, description }}");
        string formattedExpected = @"
        {
            ""fql"": [
                ""Product.where(.quantity == "",
                {
                    ""fql"": [
                        ""let product = Product.firstWhere(.backorderLimit == "",
                        { ""value"": { ""@int"": ""15""} },
                        "" \u0026\u0026 .backordered == "",
                        { ""value"": false },
                        "")!; product.quantity;""
                    ]
                },
                "").order(.title) { name, description }""
            ]
        }";
        string expected = Regex.Replace(formattedExpected, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");

        var actual = mainQuery.Serialize();

        Assert.AreEqual(expected, actual);
    }
}
