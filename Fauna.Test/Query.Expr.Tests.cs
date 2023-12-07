using NUnit.Framework;
using static Fauna.Query;

namespace Fauna.Test;

[TestFixture]
public class QueryExprTests
{
    [Test]
    public void Ctor_WithList()
    {
        var expected = new List<object> { "item1", "item2" };

        var actual = new Expr(expected);

        Assert.AreEqual(expected.AsReadOnly(), actual.Fragments);
    }

    [Test]
    public void Ctor_WithParamsArray()
    {
        var item1 = "item1";
        var item2 = "item1";
        var expected = new List<string> { item1, item2 }.AsReadOnly();

        var actual = new Expr(item1, item2);

        Assert.AreEqual(expected, actual.Fragments);
    }

    [Test]
    public void Ctor_WithValidFragments_ShouldNotThrowException()
    {
        var validFragments = new object[]
        {
            "fragment1",
            new Expr(new Val<string>("fragment2")),
            new Val<int>(123)
        };

        Assert.DoesNotThrow(() => new Expr(validFragments));
    }

    [Test]
    public void Ctor_WithInvalidFragments_ShouldThrowArgumentException()
    {
        var invalidFragments = new object[]
        {
            75,
            new List<string>()
        };

        foreach (var fragment in invalidFragments)
        {
            Assert.Throws<ArgumentException>(() => new Expr(fragment));
        }
    }

    [Test]
    public void Ctor_WithNullFragments_ShouldThrowArgumentNullException()
    {
        #nullable disable
        Assert.Throws<ArgumentNullException>(() => new Expr((IList<object>)null));
        #nullable restore
    }

    [Test]
    public void ToString_ReturnsExpectedValue()
    {
        var expr = new Expr("item1", "item2");
        var expectedString = "Expr(item1,item2)";
        Assert.AreEqual(expectedString, expr.ToString());
    }

    [Test]
    public void GetHashCode_ReturnsConsistentValue()
    {
        var val = new Query.Val<int>(143);
        int hashCode1 = val.GetHashCode();
        int hashCode2 = val.GetHashCode();
        Assert.AreEqual(hashCode1, hashCode2);
    }

    [Test]
    public void Equality_WithSameValue()
    {
        var expr1 = new Expr("item1", "item2");
        var expr2 = new Expr("item1", "item2");
        Assert.IsTrue(expr1.Equals(expr2));
    }

    [Test]
    public void Inequality_WithDifferentValue()
    {
        var expr1 = new Expr("item1", "item2");
        var expr2 = new Expr("item3", "item4");
        Assert.IsFalse(expr1.Equals(expr2));
    }

    [Test]
    public void EqualityOperator_WithSameValue()
    {
        var expr1 = new Expr("item1", "item2");
        var expr2 = new Expr("item1", "item2");
        Assert.IsTrue(expr1 == expr2);
    }

    [Test]
    public void InequalityOperator_WithDifferentValue()
    {
        var expr1 = new Expr("item1", "item2");
        var expr2 = new Expr("item3", "item4");
        Assert.IsTrue(expr1 != expr2);

    }
}
