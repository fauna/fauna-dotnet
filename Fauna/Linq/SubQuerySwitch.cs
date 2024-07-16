using System.Linq.Expressions;
using System.Reflection;
using Fauna.Util;
using QH = Fauna.Linq.IntermediateQueryHelpers;

namespace Fauna.Linq;

internal class SubQuerySwitch : DefaultExpressionSwitch<Query>
{
    private readonly LookupTable _lookup;

    public SubQuerySwitch(LookupTable lookup)
    {
        _lookup = lookup;
    }

    protected override Query ApplyDefault(Expression? expr) =>
        throw IQuerySource.Fail(expr);

    protected override Query ConstantExpr(ConstantExpression expr)
    {
        return expr.Value switch
        {
            DataContext.ICollection col => QH.CollectionAll(col),
            DataContext.IIndex idx => QH.CollectionIndex(idx),
            _ => QH.Const(expr.Value)
        };
    }

    protected override Query LambdaExpr(LambdaExpression expr)
    {
        var ps = expr.Parameters;
        var pinner = string.Join(", ", ps.Select(p => p.Name));
        var param = ps.Count() == 1 ? pinner : $"({pinner})";
        var arrow = QH.Expr($"{param} =>");

        return arrow.Concat(QH.Parens(Apply(expr.Body)));
    }

    protected override Query ParameterExpr(ParameterExpression expr) => QH.Expr(expr.Name!);

    protected override Query BinaryExpr(BinaryExpression expr)
    {
        var op = expr.NodeType switch
        {
            ExpressionType.Add => "+",
            ExpressionType.AddChecked => "+",
            ExpressionType.And => "&", // bitwise
            ExpressionType.AndAlso => "&&", // boolean
                                            // ExpressionType.ArrayIndex => ,
            ExpressionType.Coalesce => "??",
            ExpressionType.Divide => "/",
            ExpressionType.Equal => "==",
            ExpressionType.ExclusiveOr => "^",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LeftShift => "<<",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.Modulo => "%",
            ExpressionType.Multiply => "*",
            ExpressionType.MultiplyChecked => "*",
            ExpressionType.NotEqual => "!=",
            ExpressionType.Or => "|", // bitwise
            ExpressionType.OrElse => "||", // boolean
            ExpressionType.Power => "**",
            ExpressionType.RightShift => ">>",
            ExpressionType.Subtract => "-",
            ExpressionType.SubtractChecked => "-",
            _ => throw IQuerySource.Fail(expr)
        };

        var lhs = Apply(expr.Left);
        var rhs = Apply(expr.Right);

        return QH.Parens(QH.Op(lhs, op, rhs));
    }

    protected override Query CallExpr(MethodCallExpression expr)
    {
        var (callee, args, ext) = Expressions.GetCalleeAndArgs(expr);
        var name = _lookup.MethodLookup(expr.Method, callee)?.Name;
        if (name is null) throw IQuerySource.Fail(expr);
        return QH.MethodCall(Apply(callee), name, ApplyAll(args));
    }

    protected override Query MemberAccessExpr(MemberExpression expr)
    {
        var callee = expr.Expression;
        if (callee is null)
        {
            var val = Expression.Lambda(expr).Compile().DynamicInvoke();
            return QH.Const(val);
        }
        else if (callee.Type.IsClosureType())
        {
            var val = Expression.Lambda(expr).Compile().DynamicInvoke();
            return QH.Const(val);
        }

        switch (Apply(callee))
        {
            case QueryVal v:
                var c = Expression.Constant(v.Unwrap);
                var access = Expression.PropertyOrField(c, expr.Member.Name);
                var val = Expression.Lambda(access).Compile().DynamicInvoke();
                return QH.Const(val);

            case var q:
                var name = expr.Member is PropertyInfo prop ?
                    _lookup.FieldLookup(prop, callee)?.Name :
                    null;

                if (name is null) throw IQuerySource.Fail(expr);
                return QH.FieldAccess(q, name);
        }
    }
}
