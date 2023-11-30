using NUnit.Framework;

namespace Fauna.Test;

[TestFixture]
public class ClientConfigTests
{
    [Test]
    public void ConstructorWorksFine()
    {
        var b = new ClientConfig { Secret = "secret" };

        Assert.AreEqual("secret", b.Secret);
        Assert.AreEqual(Constants.Endpoints.Default, b.Endpoint);
    }
}
