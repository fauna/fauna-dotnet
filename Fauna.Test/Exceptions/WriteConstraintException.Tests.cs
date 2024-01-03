using Fauna.Exceptions;
using Fauna.Test.Helpers;
using NUnit.Framework;

namespace Fauna.Test.Exceptions
{
    [TestFixture]
    public class WriteConstraintExceptionTests
    {
        [Test]
        public void CtorWithQueryFailureAndMessage_ShouldSetProperties()
        {
            var queryFailure = ExceptionTestHelper.CreateQueryFailure("write_constraint_error");
            var message = "Write constraint error";
            var exception = new WriteConstraintException(queryFailure, message);

            Assert.AreEqual(exception.QueryFailure, queryFailure);
            Assert.AreEqual(exception.Message, message);
        }

        [Test]
        public void CtorWithQueryFailureMessageAndInnerException_ShouldSetProperties()
        {
            var queryFailure = ExceptionTestHelper.CreateQueryFailure("write_constraint_error");
            var message = "Write constraint error";
            var innerException = new Exception("Inner exception");
            var exception = new WriteConstraintException(queryFailure, message, innerException);

            Assert.AreEqual(exception.QueryFailure, queryFailure);
            Assert.AreEqual(exception.Message, message);
            Assert.AreEqual(exception.InnerException, innerException);
        }
    }
}
