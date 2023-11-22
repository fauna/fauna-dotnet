using NUnit.Framework;

namespace Fauna.Client.Test;

[TestFixture]
public class HelloWorldTests
{
  [Test]
  public void ConstructorWorksFine()
  {
    var helloWorld = new HelloWorld() { MyField = "Yep!" };
    Assert.AreEqual("Yep!", helloWorld.MyField);
  }
}
