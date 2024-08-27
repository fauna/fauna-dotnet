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
#nullable disable
        Assert.Throws<ArgumentNullException>(() => new QueryLiteral(literal));
#nullable restore
    }

    [Test]
    public void Ctor_WithLongString()
    {
        var longString = new string('a', 10000);
        Assert.DoesNotThrow(() => new QueryLiteral(longString));
    }

    [TestCase(" ")]
    [TestCase("\t")]
    [TestCase("\n")]
    public void Ctor_WithWhitespaceString(string expected)
    {
        var queryLiteral = new QueryLiteral(expected);
        Assert.AreEqual(expected, queryLiteral.Unwrap);
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
    public void GetHashCode_DifferentValues_ShouldProduceDifferentHashCodes()
    {
        var literal1 = new QueryLiteral("test1");
        var literal2 = new QueryLiteral("test2");
        Assert.AreNotEqual(literal1.GetHashCode(), literal2.GetHashCode());
    }

    [Test]
    public void Serialize_ReturnsCorrectStringValue()
    {
        var queryLiteral = new QueryLiteral("test value");
        var expected = "\"test value\"";
        var actual = queryLiteral.Serialize();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Serialize_CorrectlyEncodesNewLineCharacters()
    {
        var newline = Environment.NewLine;
        var expectedNewline = newline.Replace("\r", "\\r").Replace("\n", "\\n");
        var queryLiteral = new QueryLiteral("test" + newline + "value");
        var expected = $"\"test{expectedNewline}value\"";
        var actual = queryLiteral.Serialize();
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void Serialize_WithSpecialCharacters()
    {
        var stringValue = "Line1\nLine2\t\"Quote\" and a \\ backslash";
        var expected = @"""Line1\nLine2\t\u0022Quote\u0022 and a \\ backslash""";
        var queryLiteral = new QueryLiteral(stringValue);
        var actual = queryLiteral.Serialize();
        Assert.AreEqual(expected, actual);
    }
}
