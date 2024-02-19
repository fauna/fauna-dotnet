using System.Linq.Expressions;

namespace Fauna.Util;

internal static class Expressions
{
    public static (Expression, Expression[], bool) GetCalleeAndArgs(MethodCallExpression expr) =>
         expr.Object switch
         {
             null => (expr.Arguments.First(), expr.Arguments.Skip(1).ToArray(), true),
             var c => (c, expr.Arguments.ToArray(), false),
         };

    public static LambdaExpression? UnwrapLambda(Expression expr) =>
    expr.NodeType switch
    {
        ExpressionType.Lambda => (LambdaExpression)expr,

        ExpressionType.Convert or
        ExpressionType.ConvertChecked or
        ExpressionType.Quote =>
            UnwrapLambda(((UnaryExpression)expr).Operand),
        _ => null,
    };
}
