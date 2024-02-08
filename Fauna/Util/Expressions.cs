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

    public static object[] FindAllClosures(Expression expr)
    {
        var finder = new ClosureFinderVisitor();
        finder.Visit(expr);
        return finder.closures.ToArray();
    }

    private class ClosureFinderVisitor : ExpressionVisitor
    {
        // most exprs have one closure at most
        HashSet<object> seen = new HashSet<object>(1);
        public List<object> closures = new List<object>(1);

        protected override Expression VisitConstant(ConstantExpression node)
        {
            // node.Value cannot be null if closure class shape checks out.
            if (node.Type.IsClosureType() && !seen.Contains(node.Value!))
            {
                seen.Add(node.Value!);
                closures.Add(node.Value!);
            }

            return base.VisitConstant(node);
        }
    }

    public static Expression SubstituteByType(Expression expr, Dictionary<Type, Expression> subs)
    {
        var visitor = new SubstitutionVisitor(subs);
        return visitor.Visit(expr);
    }

    private class SubstitutionVisitor : ExpressionVisitor
    {
        private readonly Dictionary<Type, Expression> _subs;

        public SubstitutionVisitor(Dictionary<Type, Expression> subs)
        {
            _subs = subs;
        }

        protected override Expression VisitConstant(ConstantExpression expr)
        {
            if (_subs.TryGetValue(expr.Type, out var subexpr))
            {
                return subexpr;
            }
            return expr;
        }
    }

}
