using Fauna.Exceptions;
using Fauna.Test.Helpers;
using NUnit.Framework;

namespace Fauna.Test.Exceptions
{
    [TestFixture]
    public class AuthorizationExceptionTests
    {
        [Test]
        public void CtorWithQueryFailureAndMessage_ShouldSetProperties()
        {
            var queryFailure = ExceptionTestHelper.CreateQueryFailure("forbidden");
            var message = "Authorization error";
            var exception = new AuthorizationException(queryFailure, message);

            Assert.AreEqual(exception.QueryFailure, queryFailure);
            Assert.AreEqual(exception.Message, message);
        }

        [Test]
        public void CtorWithQueryFailureMessageAndInnerException_ShouldSetProperties()
        {
            var queryFailure = ExceptionTestHelper.CreateQueryFailure("forbidden");
            var message = "Authorization error";
            var innerException = new Exception("Inner exception");
            var exception = new AuthorizationException(queryFailure, message, innerException);

            Assert.AreEqual(exception.QueryFailure, queryFailure);
            Assert.AreEqual(exception.Message, message);
            Assert.AreEqual(exception.InnerException, innerException);
        }
    }
}
