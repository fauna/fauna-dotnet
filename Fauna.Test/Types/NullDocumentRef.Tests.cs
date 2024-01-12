using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Types;

[TestFixture]
public class NullDocumentRefTests
{

    [Test]
    public void CauseDefaultTest()
    {
        var doc = new NullDocumentRef();
        Assert.IsNull(doc.Cause);
    }
}
