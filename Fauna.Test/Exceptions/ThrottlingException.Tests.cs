using Fauna.Exceptions;
using Fauna.Test.Helpers;
using NUnit.Framework;

namespace Fauna.Test.Exceptions
{
    [TestFixture]
    public class ThrottlingExceptionTests
    {
        [Test]
        public void CtorWithQueryFailureAndMessage_ShouldSetProperties()
        {
            var queryFailure = ExceptionTestHelper.CreateQueryFailure("limit_exceeded");
            var message = "Throttling error occurred";
            var exception = new ThrottlingException(queryFailure, message);

            Assert.AreEqual(exception.QueryFailure, queryFailure);
            Assert.AreEqual(exception.Message, message);
        }

        [Test]
        public void CtorWithQueryFailureMessageAndInnerException_ShouldSetProperties()
        {
            var queryFailure = ExceptionTestHelper.CreateQueryFailure("limit_exceeded");
            var message = "Throttling error occurred";
            var innerException = new Exception("Inner exception");
            var exception = new ThrottlingException(queryFailure, message, innerException);

            Assert.AreEqual(exception.QueryFailure, queryFailure);
            Assert.AreEqual(exception.Message, message);
            Assert.AreEqual(exception.InnerException, innerException);
        }
    }
}
