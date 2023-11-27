using NUnit.Framework;
using Fauna.Configuration;

namespace Fauna.Test;

[TestFixture]
public class ClientTests
{
  [Test]
  public void ConstructorWorksFine()
  {
    var b = Config.CreateBuilder().SetSecret("secret");

    Assert.AreEqual("secret", b.Secret);
  }
}
