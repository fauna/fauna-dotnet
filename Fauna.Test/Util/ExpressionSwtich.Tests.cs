using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Fauna.Util;
using NUnit.Framework;

namespace Fauna.Test.Util
{
    internal class ExpressionSwitchTest : ExpressionSwitch<string>
    {
        protected override string NullExpr() => "NullExpr";

        protected override string BinaryExpr(BinaryExpression expr) => "BinaryExpr";
        protected override string BlockExpr(BlockExpression expr) => "BlockExpr";
        protected override string ConditionalExpr(ConditionalExpression expr) => "ConditionalExpr";
        protected override string CallExpr(MethodCallExpression expr) => "CallExpr";
        protected override string ConstantExpr(ConstantExpression expr) => "ConstantExpr";
        protected override string DebugInfoExpr(DebugInfoExpression expr) => "DebugInfoExpr";
        protected override string DefaultExpr(DefaultExpression expr) => "DefaultExpr";
        protected override string DynamicExpr(DynamicExpression expr) => "DynamicExpr";
        protected override string GotoExpr(GotoExpression expr) => "GotoExpr";
        protected override string IndexExpr(IndexExpression expr) => "IndexExpr";
        protected override string InvokeExpr(InvocationExpression expr) => "InvokeExpr";
        protected override string LabelExpr(LabelExpression expr) => "LabelExpr";
        protected override string LambdaExpr(LambdaExpression expr) => "LambdaExpr";
        protected override string ListInitExpr(ListInitExpression expr) => "ListInitExpr";
        protected override string LoopExpr(LoopExpression expr) => "LoopExpr";
        protected override string MemberAccessExpr(MemberExpression expr) => "MemberAccessExpr";
        protected override string MemberInitExpr(MemberInitExpression expr) => "MemberInitExpr";
        protected override string NewArrayExpr(NewArrayExpression expr) => "NewArrayExpr";
        protected override string NewExpr(NewExpression expr) => "NewExpr";
        protected override string ParameterExpr(ParameterExpression expr) => "ParameterExpr";
        protected override string RuntimeVariablesExpr(RuntimeVariablesExpression expr) => "RuntimeVariablesExpr";
        protected override string SwitchExpr(SwitchExpression expr) => "SwitchExpr";
        protected override string TryExpr(TryExpression expr) => "TryExpr";
        protected override string TypeBinaryExpr(TypeBinaryExpression expr) => "TypeBinaryExpr";
        protected override string UnaryExpr(UnaryExpression expr) => "UnaryExpr";
        protected override string UnknownExpr(Expression expr) => "UnknownExpr";
    }

    [TestFixture]
    public class ExpressionSwitchTests
    {
        [AllowNull]
        private ExpressionSwitchTest _expressionSwitch;

        [SetUp]
        public void SetUp()
        {
            _expressionSwitch = new ExpressionSwitchTest();
        }

        [Test]
        public void Apply_NullExpression_ReturnsNullExpr()
        {
            var result = _expressionSwitch.Apply(null);

            Assert.AreEqual("NullExpr", result);
        }

        [Test]
        public void Apply_ConstantExpression_ReturnsConstantExpr()
        {
            var expression = Expression.Constant(42);

            var result = _expressionSwitch.Apply(expression);

            Assert.AreEqual("ConstantExpr", result);
        }

        [Test]
        public void Apply_BinaryExpression_ReturnsBinaryExpr()
        {
            var left = Expression.Constant(1);
            var right = Expression.Constant(2);

            var expression = Expression.Add(left, right);
            var result = _expressionSwitch.Apply(expression);

            Assert.AreEqual("BinaryExpr", result);
        }

        [Test]
        public void Apply_MethodCallExpression_ReturnsCallExpr()
        {
            var instance = Expression.Constant(new object());
            var methodInfo = typeof(object).GetMethod("ToString", Type.EmptyTypes);
            if (methodInfo == null)
            {
                Assert.Fail("methodInfo for object.ToString() should not be null");
                return;
            }

            var expression = Expression.Call(instance, methodInfo);
            var result = _expressionSwitch.Apply(expression);

            Assert.AreEqual("CallExpr", result);
        }

        [Test]
        public void Apply_UnaryExpression_ReturnsUnaryExpr()
        {
            var operand = Expression.Constant(1);
            var expression = Expression.Negate(operand);

            var result = _expressionSwitch.Apply(expression);

            Assert.AreEqual("UnaryExpr", result);
        }
    }
}
