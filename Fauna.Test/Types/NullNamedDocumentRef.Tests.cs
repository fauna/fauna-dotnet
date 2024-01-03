using Fauna.Types;
using NUnit.Framework;

namespace Fauna.Test.Types;

[TestFixture]
public class NullNamedDocumentRefTests
{

    [Test]
    public void ExistsAndReasonDefaultTest()
    {
        var doc = new NullNamedDocumentRef();
        Assert.IsNull(doc.Reason);
    }
}