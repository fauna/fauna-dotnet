using NUnit.Framework;

namespace Fauna.Test;

[TestFixture]
public class QueryValTests
{
    [Test]
    public void Ctor_WrapsFloatCorrectly()
    {
        float expected = float.MaxValue;
        var actual = new QueryVal<float>(expected);
        Assert.AreEqual(expected, actual.Unwrap);
        Assert.IsInstanceOf<float>(actual.Unwrap);
    }

    [Test]
    public void Ctor_WrapsIntCorrectly()
    {
        int expected = int.MaxValue;
        var actual = new QueryVal<int>(expected);
        Assert.AreEqual(expected, actual.Unwrap);
        Assert.IsInstanceOf<int>(actual.Unwrap);
    }

    [Test]
    public void Ctor_WrapsDateTimeCorrectly()
    {
        DateTime expected = DateTime.Now;
        var actual = new QueryVal<DateTime>(expected);
        Assert.AreEqual(expected, actual.Unwrap);
        Assert.IsInstanceOf<DateTime>(actual.Unwrap);
    }

    [Test]
    public void Ctor_WrapsDateTimeOffsetCorrectly()
    {
        DateTimeOffset expected = DateTimeOffset.Now;
        var actual = new QueryVal<DateTimeOffset>(expected);
        Assert.AreEqual(expected, actual.Unwrap);
        Assert.IsInstanceOf<DateTimeOffset>(actual.Unwrap);
    }

    [Test]
    public void Ctor_WrapsStringCorrectly()
    {
        string expected = "test string";
        var actual = new QueryVal<string>(expected);
        Assert.AreEqual(expected, actual.Unwrap);
        Assert.IsInstanceOf<string>(actual.Unwrap);
    }

    [Test]
    public void Ctor_WrapsNullCorrectly()
    {
        var actual = new QueryVal<string?>(null);
        Assert.IsNull(actual.Unwrap);
    }

    [Test]
    public void ToString_ReturnsExpectedValue()
    {
        var val = new QueryVal<int>(143);
        Assert.AreEqual("QueryVal(143)", val.ToString());
    }

    [Test]
    public void GetHashCode_ReturnsConsistentValue()
    {
        var val = new QueryVal<int>(143);
        int hashCode1 = val.GetHashCode();
        int hashCode2 = val.GetHashCode();
        Assert.AreEqual(hashCode1, hashCode2);
    }

    [Test]
    public void Equality_WithSameValue()
    {
        var val1 = new QueryVal<int>(100);
        var val2 = new QueryVal<int>(100);

        Assert.AreEqual(val1, val2);
    }

    [Test]
    public void Inequality_WithDifferentValueOrType()
    {
        var val1 = new QueryVal<int>(100);
        var val2 = new QueryVal<int>(200);
        var val3 = new QueryVal<short>(100);

        Assert.AreNotEqual(val1, val2);
        Assert.AreNotEqual(val1, val3);
    }

    [Test]
    public void EqualityOperator_WithSameValue()
    {
        var val1 = new QueryVal<int>(100);
        var val2 = new QueryVal<int>(100);

        Assert.IsTrue(val1 == val2);
    }

    [Test]
    public void InequalityOperator_WithDifferentValue()
    {
        var val1 = new QueryVal<int>(100);
        var val2 = new QueryVal<int>(200);

        Assert.IsTrue(val1 != val2);
    }
}
