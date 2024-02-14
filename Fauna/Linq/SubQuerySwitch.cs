using System.Linq.Expressions;
using System.Reflection;
using Fauna.Util;

namespace Fauna.Linq;

using IE = IntermediateExpr;

internal class SubQuerySwitch : DefaultExpressionSwitch<IE>
{
    private readonly LookupTable _lookup;

    public SubQuerySwitch(LookupTable lookup)
    {
        _lookup = lookup;
    }

    protected override IE ApplyDefault(Expression? expr) =>
        throw IQuerySource.Fail(expr);

    protected override IE ConstantExpr(ConstantExpression expr)
    {
        if (expr.Value is DataContext.Collection col)
        {
            return IE.CollectionAll(col);
        }
        else if (expr.Value is DataContext.Index idx)
        {
            return IE.CollectionIndex(idx);
        }
        else
        {
            return IE.Const(expr.Value);
        }
    }

    protected override IE LambdaExpr(LambdaExpression expr)
    {
        var ps = expr.Parameters;
        var pinner = string.Join(", ", ps.Select(p => p.Name));
        var param = ps.Count() == 1 ? pinner : $"({pinner})";
        var arrow = IE.Exp($"{param} =>");

        return arrow.Concat(IE.Parens(Apply(expr.Body)));
    }

    protected override IE ParameterExpr(ParameterExpression expr) => IE.Exp(expr.Name!);

    protected override IE BinaryExpr(BinaryExpression expr)
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

        return IE.Parens(IE.Op(lhs, op, rhs));
    }

    protected override IE CallExpr(MethodCallExpression expr)
    {
        var (callee, args, ext) = Expressions.GetCalleeAndArgs(expr);
        var name = _lookup.MethodLookup(expr.Method, callee)?.Name;
        if (name is null) throw IQuerySource.Fail(expr);
        return IE.MethodCall(Apply(callee), name, ApplyAll(args));
    }

    protected override IE MemberAccessExpr(MemberExpression expr)
    {
        var callee = expr.Expression;
        if (callee is null)
        {
            var val = Expression.Lambda(expr).Compile().DynamicInvoke();
            return IE.Const(val);
        }
        else if (callee.Type.IsClosureType())
        {
            return Apply(callee).Access(expr.Member.Name);
        }
        else
        {
            var name = expr.Member is PropertyInfo prop ?
                _lookup.FieldLookup(prop, callee)?.Name :
                null;

            if (name is null) throw IQuerySource.Fail(expr);
            return Apply(callee).Access(name);
        }
    }
}
