using Fauna.Exceptions;
using NUnit.Framework;

namespace Fauna.Test.Exceptions
{
    [TestFixture]
    public class ProtocolExceptionTests
    {
        [Test]
        public void CtorWithMessage_ShouldSetMessage()
        {
            var message = "Protocol error";
            var exception = new ProtocolException(message);
            Assert.AreEqual(exception.Message, message);
        }

        [Test]
        public void CtorWithMessageAndInnerException_ShouldSetProperties()
        {
            var message = "Protocol error";
            var innerException = new Exception("Inner exception");
            var exception = new ProtocolException(message, innerException);

            Assert.AreEqual(exception.Message, message);
            Assert.AreEqual(exception.InnerException, innerException);
        }
    }
}
