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
            new Query.Val("foo"),
            " && .genre == ",
            new Query.Val("bar"),
            ")"
        );

        Assert.That(q, Is.EqualTo(expected));
    }
}
