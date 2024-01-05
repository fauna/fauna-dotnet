using Fauna.Exceptions;
using Fauna.Test.Helpers;
using NUnit.Framework;

namespace Fauna.Test.Exceptions
{
    [TestFixture]
    public class ServiceExceptionTests
    {
        [Test]
        public void CtorWithQueryFailureAndMessage_ShouldSetProperties()
        {
            var queryFailure = ExceptionTestHelper.CreateQueryFailure("internal_error");
            var message = "Service error";
            var exception = new ServiceException(queryFailure, message);

            Assert.AreEqual(exception.QueryFailure, queryFailure);
            Assert.AreEqual(exception.Message, message);
        }
    }
}
