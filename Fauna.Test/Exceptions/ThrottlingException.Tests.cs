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
    }
}
