using System.Linq.Expressions;
using Fauna.Util;
using NUnit.Framework;

namespace Fauna.Test.Util
{
    [TestFixture]
    public class ExpressionsTests
    {
        [Test]
        public void GetCalleeAndArgs_WithObjectNull_ReturnsCorrectTuple()
        {
            var arg1 = Expression.Constant(1);
            var arg2 = Expression.Constant(2);
            var methodInfo = typeof(Math).GetMethod("Max", new[] { typeof(int), typeof(int) });
            if (methodInfo == null)
            {
                Assert.Fail("methodInfo for Math.Max(int, int) should not be null");
                return;
            }

            var methodCallExpression = Expression.Call(null, methodInfo, arg1, arg2);
            var (callee, args, isStatic) = Expressions.GetCalleeAndArgs(methodCallExpression);

            Assert.AreEqual(arg1, callee);
            Assert.AreEqual(new[] { arg2 }, args);
            Assert.IsTrue(isStatic);
        }

        [Test]
        public void GetCalleeAndArgs_WithObjectNotNull_ReturnsCorrectTuple()
        {
            var instance = Expression.Constant(new object());
            var methodInfo = typeof(object).GetMethod("ToString", Type.EmptyTypes);
            if (methodInfo == null)
            {
                Assert.Fail("methodInfo for object.ToString() should not be null");
                return; // Just in case Assert.Fail does not stop execution
            }

            var methodCallExpression = Expression.Call(instance, methodInfo);
            var (callee, args, isStatic) = Expressions.GetCalleeAndArgs(methodCallExpression);

            Assert.AreEqual(instance, callee);
            Assert.AreEqual(new Expression[] { }, args);
            Assert.IsFalse(isStatic);
        }

        [Test]
        public void UnwrapLambda_WithLambdaExpression_ReturnsLambdaExpression()
        {
            Expression<Func<int>> lambda = () => 1;
            var result = Expressions.UnwrapLambda(lambda);

            Assert.AreEqual(lambda, result);
        }

        [Test]
        public void UnwrapLambda_WithConvertExpression_ReturnsLambdaExpression()
        {
            Expression<Func<int>> lambda = () => 1;
            var convertExpression = Expression.Convert(lambda, typeof(object));
            var result = Expressions.UnwrapLambda(convertExpression);

            Assert.AreEqual(lambda, result);
        }

        [Test]
        public void UnwrapLambda_WithQuoteExpression_ReturnsLambdaExpression()
        {
            Expression<Func<int>> lambda = () => 1;
            var quoteExpression = Expression.Quote(lambda);
            var result = Expressions.UnwrapLambda(quoteExpression);

            Assert.AreEqual(lambda, result);
        }

        [Test]
        public void UnwrapLambda_WithOtherExpression_ReturnsNull()
        {
            var constantExpression = Expression.Constant(1);
            var result = Expressions.UnwrapLambda(constantExpression);

            Assert.IsNull(result);
        }
    }
}
