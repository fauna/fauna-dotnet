using System.Linq.Expressions;

namespace Fauna.Util;

internal class Expressions
{
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
