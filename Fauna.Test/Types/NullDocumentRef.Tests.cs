using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Types;

[TestFixture]
public class NullDocumentRefTests
{

    [Test]
    public void ReasonDefaultTest()
    {
        var doc = new NullDocumentRef();
        Assert.IsNull(doc.Reason);
    }
}