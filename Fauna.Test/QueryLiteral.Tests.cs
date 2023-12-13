using NUnit.Framework;

namespace Fauna.Test;

[TestFixture]
public class QueryLiteralTests
{
    [Test]
    public void Ctor_WithValidValue()
    {
        string expected = "test value";
        var queryLiteral = new QueryLiteral(expected);
        Assert.AreEqual(expected, queryLiteral.Unwrap);
    }

    [Test]
    public void Ctor_WithNullValue_ThrowsArgumentNullException()
    {
        string? literal = null;
        Assert.Throws<ArgumentNullException>(() => new QueryLiteral(literal));
    }

    [Test]
    public void ToString_ReturnsExpectedValue()
    {
        var queryLiteral = new QueryLiteral("test value");
        var expectedString = "QueryLiteral(test value)";
        Assert.AreEqual(expectedString, queryLiteral.ToString());
    }

    [Test]
    public void Equals_WithSameValue()
    {
        var literal1 = new QueryLiteral("test");
        var literal2 = new QueryLiteral("test");
        Assert.IsTrue(literal1.Equals(literal2));
    }

    [Test]
    public void Equals_WithDifferentValue()
    {
        var literal1 = new QueryLiteral("test1");
        var literal2 = new QueryLiteral("test2");
        Assert.IsFalse(literal1.Equals(literal2));
    }

    [Test]
    public void EqualityOperator_WithSameValue()
    {
        var literal1 = new QueryLiteral("test");
        var literal2 = new QueryLiteral("test");
        Assert.IsTrue(literal1 == literal2);
    }

    [Test]
    public void InequalityOperator_WithDifferentValue()
    {
        var literal1 = new QueryLiteral("test1");
        var literal2 = new QueryLiteral("test2");
        Assert.IsTrue(literal1 != literal2);
    }

    [Test]
    public void GetHashCode_ReturnsConsistentValue()
    {
        var literal = new QueryLiteral("test");
        int hashCode1 = literal.GetHashCode();
        int hashCode2 = literal.GetHashCode();
        Assert.AreEqual(hashCode1, hashCode2);
    }

    [Test]
    public void Serialize_ReturnsCorrectStringValue()
    {
        var queryLiteral = new QueryLiteral("test value");
        var expected = "\"test value\"";
        var actual = queryLiteral.Serialize();
        Assert.AreEqual(expected, actual);
    }
}
