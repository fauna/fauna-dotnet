using Fauna.Exceptions;
using NUnit.Framework;

namespace Fauna.Test.Exceptions
{
    [TestFixture]
    public class FaunaExceptionTests
    {
        [Test]
        public void Ctor_ShouldInstantiate()
        {
            var exception = new FaunaException();
            Assert.IsNotNull(exception);
        }

        [Test]
        public void CtorWithMessage_ShouldSetMessage()
        {
            var message = "Test message";
            var exception = new FaunaException(message);
            Assert.AreEqual(exception.Message, message);
        }

        [Test]
        public void CtorWithMessageAndInnerException_ShouldSetProperties()
        {
            var message = "Test message";
            var innerException = new Exception("Inner exception");
            var exception = new FaunaException(message, innerException);

            Assert.AreEqual(exception.Message, message);
            Assert.AreEqual(exception.InnerException, innerException);
        }
    }

}
