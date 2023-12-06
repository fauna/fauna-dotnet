using NUnit.Framework;

namespace Fauna.Test;

[TestFixture]
public class ClientConfigTests
{
    [Test]
    public void ConstructorWorksFine()
    {
        var b = new ClientConfig("secret");

        Assert.AreEqual("secret", b.Secret);
        Assert.AreEqual(Constants.Endpoints.Default, b.Endpoint);
    }
}
