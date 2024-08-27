using Fauna.QueryFragments;
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
        var actual = new QueryVal(expected);
        Assert.AreEqual(expected, actual.Unwrap);
        Assert.IsInstanceOf<float>(actual.Unwrap);
    }

    [Test]
    public void Ctor_WrapsIntCorrectly()
    {
        int expected = int.MaxValue;
        var actual = new QueryVal(expected);
        Assert.AreEqual(expected, actual.Unwrap);
        Assert.IsInstanceOf<int>(actual.Unwrap);
    }

    [Test]
    public void Ctor_WrapsDateTimeCorrectly()
    {
        DateTime expected = DateTime.Now;
        var actual = new QueryVal(expected);
        Assert.AreEqual(expected, actual.Unwrap);
        Assert.IsInstanceOf<DateTime>(actual.Unwrap);
    }

    [Test]
    public void Ctor_WrapsDateTimeOffsetCorrectly()
    {
        DateTimeOffset expected = DateTimeOffset.Now;
        var actual = new QueryVal(expected);
        Assert.AreEqual(expected, actual.Unwrap);
        Assert.IsInstanceOf<DateTimeOffset>(actual.Unwrap);
    }

    [Test]
    public void Ctor_WrapsStringCorrectly()
    {
        string expected = "test string";
        var actual = new QueryVal(expected);
        Assert.AreEqual(expected, actual.Unwrap);
        Assert.IsInstanceOf<string>(actual.Unwrap);
    }

    [Test]
    public void Ctor_WrapsNullCorrectly()
    {
        var actual = new QueryVal(null);
        Assert.IsNull(actual.Unwrap);
    }

    [TestCase(" ")]
    [TestCase("\t")]
    [TestCase("\n")]
    public void Ctor_WrapsWhitespaceString(string expected)
    {
        var queryLiteral = new QueryVal(expected);
        Assert.AreEqual(expected, queryLiteral.Unwrap);
    }

    [Test]
    public void Ctor_WrapsPoco()
    {
        string property1 = "value123";
        int property2 = 123;
        var poco = new TestPoco { Property1 = property1, Property2 = property2 };
        var queryVal = new QueryVal(poco);
        var unwrapped = (TestPoco)queryVal.Unwrap!;
        Assert.AreEqual(property1, unwrapped.Property1);
        Assert.AreEqual(property2, unwrapped.Property2);
    }

    [Test]
    public void Ctor_WrapsDictionary()
    {
        string value1 = "value123";
        int value2 = 123;
        var dictionary = new Dictionary<string, object> { { "key1", value1 }, { "key2", value2 } };
        var queryVal = new QueryVal(dictionary);
        var unwrapped = (Dictionary<string, object>)queryVal.Unwrap!;
        Assert.AreEqual(2, unwrapped.Count);
        Assert.AreEqual(value1, unwrapped["key1"]);
        Assert.AreEqual(value2, unwrapped["key2"]);
    }

    [Test]
    public void Ctor_WrapsList()
    {
        var list = new List<int> { 57, 75 };
        var queryVal = new QueryVal(list);
        var unwrapped = (List<int>)queryVal.Unwrap!;
        Assert.AreEqual(2, unwrapped.Count);
        Assert.AreEqual(57, unwrapped[0]);
        Assert.AreEqual(75, unwrapped[1]);
    }

    [Test]
    public void ToString_ReturnsExpectedValue()
    {
        var val = new QueryVal(143);
        Assert.AreEqual("QueryVal(143)", val.ToString());
    }

    [Test]
    public void GetHashCode_ReturnsConsistentValue()
    {
        var val1 = new QueryVal(143);
        var val2 = new QueryVal(143);
        int hashCode1 = val1.GetHashCode();
        int hashCode2 = val2.GetHashCode();
        Assert.AreEqual(hashCode1, hashCode2);
    }

    [Test]
    public void Equality_WithSameValue()
    {
        var val1 = new QueryVal(100);
        var val2 = new QueryVal(100);

        Assert.AreEqual(val1, val2);
    }

    [Test]
    public void Inequality_WithDifferentValueOrType()
    {
        var val1 = new QueryVal(100);
        var val2 = new QueryVal(200);
        var val3 = new QueryVal((short)100);

        Assert.AreNotEqual(val1, val2);
        Assert.AreNotEqual(val1, val3);
    }

    [Test]
    public void EqualityOperator_WithSameValue()
    {
        var val1 = new QueryVal(100);
        var val2 = new QueryVal(100);

        Assert.IsTrue(val1 == val2);
    }

    [Test]
    public void InequalityOperator_WithDifferentValue()
    {
        var val1 = new QueryVal(100);
        var val2 = new QueryVal(200);

        Assert.IsTrue(val1 != val2);
    }

    [Test]
    public void Serialize_WithIntValue()
    {
        int intValue = 5;
        string intExpected = $@"{{""value"":{{""@int"":""{intValue}""}}}}";
        var intActual = new QueryVal(intValue).Serialize();

        Assert.AreEqual(intExpected, intActual);
    }

    [Test]
    public void Serialize_WithBoolValue()
    {
        bool boolValue = true;
        string boolExpected = $@"{{""value"":{boolValue.ToString().ToLowerInvariant()}}}";
        var boolActual = new QueryVal(boolValue).Serialize();

        Assert.AreEqual(boolExpected, boolActual);
    }
}
