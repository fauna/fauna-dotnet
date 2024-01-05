using Fauna.Exceptions;
using Fauna.Test.Helpers;
using NUnit.Framework;

namespace Fauna.Test.Exceptions
{
    [TestFixture]
    public class InvalidRequestExceptionTests
    {
        [Test]
        public void CtorWithQueryFailureAndMessage_ShouldSetProperties()
        {
            var queryFailure = ExceptionTestHelper.CreateQueryFailure("invalid_request");
            var message = "Invalid request error";
            var exception = new InvalidRequestException(queryFailure, message);

            Assert.AreEqual(exception.QueryFailure, queryFailure);
            Assert.AreEqual(exception.Message, message);
        }
    }
}
