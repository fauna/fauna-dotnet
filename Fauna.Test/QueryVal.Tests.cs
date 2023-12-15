using NUnit.Framework;

namespace Fauna.Test;

[TestFixture]
public class QueryValTests
{
    class TestPoco
    {
        public string? Property1 { get; set; }
        public int Property2 { get; set; }
    }

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

    [TestCase(" ")]
    [TestCase("\t")]
    [TestCase("\n")]
    public void Ctor_WrapsWhitespaceString(string expected)
    {
        var queryLiteral = new QueryVal<string>(expected);
        Assert.AreEqual(expected, queryLiteral.Unwrap);
    }

    [Test]
    public void Ctor_WrapsPoco()
    {
        string property1 = "value123";
        int property2 = 123;
        var poco = new TestPoco { Property1 = property1, Property2 = property2 };
        var queryVal = new QueryVal<TestPoco>(poco);
        Assert.IsInstanceOf<TestPoco>(queryVal.Unwrap);
        Assert.AreEqual(property1, queryVal.Unwrap.Property1);
        Assert.AreEqual(property2, queryVal.Unwrap.Property2);
    }

    [Test]
    public void Ctor_WrapsDictionary()
    {
        string value1 = "value123";
        int value2 = 123;
        var dictionary = new Dictionary<string, object> { { "key1", value1 }, { "key2", value2 } };
        var queryVal = new QueryVal<IDictionary<string, object>>(dictionary);
        Assert.AreEqual(2, queryVal.Unwrap.Count);
        Assert.AreEqual(value1, queryVal.Unwrap["key1"]);
        Assert.AreEqual(value2, queryVal.Unwrap["key2"]);
    }

    [Test]
    public void Ctor_WrapsList()
    {
        var list = new List<int> { 57, 75 };
        var queryVal = new QueryVal<List<int>>(list);
        Assert.AreEqual(2, queryVal.Unwrap.Count);
        Assert.AreEqual(57, queryVal.Unwrap[0]);
        Assert.AreEqual(75, queryVal.Unwrap[1]);
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

    [Test]
    public void Serialize_WithIntValue()
    {
        int intValue = 5;
        string intExpected = $@"{{""value"":{{""@int"":""{intValue}""}}}}";
        var intActual = new QueryVal<int>(intValue).Serialize();

        Assert.AreEqual(intExpected, intActual);
    }

    [Test]
    public void Serialize_WithBoolValue()
    {
        bool boolValue = true;
        string boolExpected = $@"{{""value"":{boolValue.ToString().ToLowerInvariant()}}}";
        var boolActual = new QueryVal<bool>(boolValue).Serialize();

        Assert.AreEqual(boolExpected, boolActual);
    }
}
