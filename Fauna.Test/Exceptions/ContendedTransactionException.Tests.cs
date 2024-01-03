using Fauna.Exceptions;
using Fauna.Test.Helpers;
using NUnit.Framework;
using System;

namespace Fauna.Test.Exceptions
{
    [TestFixture]
    public class ContendedTransactionExceptionTests
    {
        [Test]
        public void CtorWithQueryFailureAndMessage_ShouldSetProperties()
        {
            var queryFailure = ExceptionTestHelper.CreateQueryFailure("contended_transaction");
            var message = "Transaction contention occurred";
            var exception = new ContendedTransactionException(queryFailure, message);

            Assert.AreEqual(exception.QueryFailure, queryFailure);
            Assert.AreEqual(exception.Message, message);
        }

        [Test]
        public void CtorWithQueryFailureMessageAndInnerException_ShouldSetProperties()
        {
            var queryFailure = ExceptionTestHelper.CreateQueryFailure("contended_transaction");
            var message = "Transaction contention occurred";
            var innerException = new Exception("Inner exception");
            var exception = new ContendedTransactionException(queryFailure, message, innerException);

            Assert.AreEqual(exception.QueryFailure, queryFailure);
            Assert.AreEqual(exception.Message, message);
            Assert.AreEqual(exception.InnerException, innerException);
        }
    }
}
