using Fauna.Exceptions;
using NUnit.Framework;

namespace Fauna.Test.Exceptions
{
    [TestFixture]
    public class NetworkExceptionTests
    {
        [Test]
        public void Ctor_ShouldInstantiate()
        {
            var exception = new NetworkException("Network error occurred");
            Assert.IsNotNull(exception);
        }

        [Test]
        public void CtorWithMessage_ShouldSetMessage()
        {
            var message = "Network error occurred";
            var exception = new NetworkException(message);
            Assert.AreEqual(exception.Message, message);
        }

        [Test]
        public void CtorWithMessageAndInnerException_ShouldSetProperties()
        {
            var message = "Network error occurred";
            var innerException = new Exception("Inner network issue");
            var exception = new NetworkException(message, innerException);

            Assert.AreEqual(exception.Message, message);
            Assert.AreEqual(exception.InnerException, innerException);
        }
    }
}
