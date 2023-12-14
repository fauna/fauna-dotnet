using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Reflection;
using LI = Fauna.LinqIntermediateExpr;

namespace Fauna;

public class LinqQueryBuilder
{
    // TODO(matt) this should return an FQL query object, not a string.
    public static Query Build(Expression expr)
    {
        var closure = ExpressionClosureFinder.Find(expr);
        var cty = closure is null ? typeof(object) : closure.GetType();
        var sw = new BuilderSwitch(cty);
        // TODO(matt) cache the result of apply for a given expr, somehow
        var ie = sw.Apply(expr);
        var param = Expression.Parameter(cty);
        var body = ie.Build(param);
        var func = Expression.Lambda(body, new[] { param }).Compile();

        return (Query)func.DynamicInvoke(closure);
    }

    private class BuilderSwitch : ExpressionSwitch<LinqIntermediateExpr>
    {
        public Type ClosureType { get; }

        public BuilderSwitch(Type cty)
        {
            ClosureType = cty;
        }

        // TODO(matt) throw an API-specific exception in-line with what other
        // LINQ libraries do.
        // FIXME(matt) what is a c# bottom/nothing type?
        private dynamic fail(Expression? expr) =>
            throw new NotSupportedException($"Unsupported {expr?.NodeType} expression: {expr}");

        protected override LinqIntermediateExpr ApplyDefault(Expression? expr) => fail(expr);

        protected override LinqIntermediateExpr ConstantExpr(ConstantExpression expr)
        {
            switch (expr.Value)
            {
                case LinqModule mod:
                    return new LI.Expr(expr.Type, mod.Module.Name);

                case string _:
                case int _:
                case long _:
                case bool _:
                    return new LI.Constant(expr.Type, expr.Value);

                default:
                    if (expr.Type == ClosureType)
                    {
                        return new LI.Closure(expr.Type);
                    }
                    break;
            }

            return fail(expr);
        }

        protected override LinqIntermediateExpr BinaryExpr(BinaryExpression expr)
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
                ExpressionType.Power => "^",
                ExpressionType.RightShift => ">>",
                ExpressionType.Subtract => "-",
                ExpressionType.SubtractChecked => "-",
                _ => fail(expr)
            };

            var lhs = Apply(expr.Left);
            var rhs = Apply(expr.Right);

            // TODO(matt) conditionally drop parens based on precedence?
            // return $"({lhs} {op} {rhs})";

            // FIXME(matt) memoize spaces up above
            return LI.Join(expr.Type, lhs, $" {op} ", rhs);
        }

        protected override LinqIntermediateExpr CallExpr(MethodCallExpression expr)
        {
            LinqIntermediateExpr BuildCall(Type ty, string methodName, Expression callee, IEnumerable<Expression> args)
            {
                // TODO(matt) wrap in parens if it's a complex expr
                var ret = Apply(callee).Concat(ty, $".{methodName}(");

                var i = 0;
                foreach (var a in args)
                {
                    Console.WriteLine(a);
                    if (i != 0) ret = ret.Concat(ty, ", ");
                    ret = ret.Concat(ty, Apply(a));
                    i += 1;
                }
                ret = ret.Concat(ty, ")");

                return ret;
            }

            switch (expr.Method.Name)
            {
                case "Where":
                    if (expr.Object != null) return fail(expr);
                    if (expr.Arguments.Count != 2) return fail(expr);
                    return BuildCall(expr.Type, "where", expr.Arguments.First(), expr.Arguments.Skip(1));
            }

            return fail(expr);
        }

        protected override LinqIntermediateExpr MemberAccessExpr(MemberExpression expr)
        {
            if (expr.Expression is null)
            {
                var val = Expression.Lambda(expr).Compile().DynamicInvoke();
                return new LI.Constant(expr.Type, val);
            }
            else
            {
                return Apply(expr.Expression).Access(expr.Type, expr.Member.Name);
            }
        }

        protected override LinqIntermediateExpr LambdaExpr(LambdaExpression expr)
        {
            var ty = expr.Type;
            var ps = expr.Parameters;
            var pinner = string.Join(", ", ps.Select(p => p.Name));
            var param = ps.Count == 1 ? pinner : $"({pinner})";
            var arrow = new LI.Expr(ty, $"{param} => ");
            var body = Apply(expr.Body);

            return arrow.Concat(ty, body);
        }

        protected override LinqIntermediateExpr ParameterExpr(ParameterExpression expr)
        {
            return new LI.Expr(expr.Type, expr.Name!);
        }
    }
}
