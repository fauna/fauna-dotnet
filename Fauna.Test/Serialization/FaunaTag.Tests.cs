using System.Buffers;
using System.Text;
using Fauna.Serialization;
using NUnit.Framework;

namespace Fauna.Test.Serialization;

[TestFixture]
public class FaunaTagTests
{
    [Test]
    public void TagsMatchStrings()
    {
        Assert.AreEqual("@date", FaunaTag.Date.ToString());
        Assert.AreEqual("@doc", FaunaTag.Document.ToString());
        Assert.AreEqual("@double", FaunaTag.Double.ToString());
        Assert.AreEqual("@int", FaunaTag.Int.ToString());
        Assert.AreEqual("@long", FaunaTag.Long.ToString());
        Assert.AreEqual("@mod", FaunaTag.Module.ToString());
        Assert.AreEqual("@object", FaunaTag.Object.ToString());
        Assert.AreEqual("@ref", FaunaTag.Ref.ToString());
        Assert.AreEqual("@set", FaunaTag.Set.ToString());
        Assert.AreEqual("@time", FaunaTag.Time.ToString());
    }
}