using Fauna.Exceptions;
using NUnit.Framework;

namespace Fauna.Test.Exceptions
{
    [TestFixture]
    public class ClientExceptionTests
    {
        [Test]
        public void CtorWithMessage_ShouldSetMessage()
        {
            var message = "Client error";
            var exception = new ClientException(message);
            Assert.AreEqual(exception.Message, message);
        }

        [Test]
        public void CtorWithMessageAndInnerException_ShouldSetProperties()
        {
            var message = "Client error";
            var innerException = new Exception("Inner exception");
            var exception = new ClientException(message, innerException);

            Assert.AreEqual(exception.Message, message);
            Assert.AreEqual(exception.InnerException, innerException);
        }
    }
}
