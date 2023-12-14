using System.Linq.Expressions;
using System.Diagnostics;

namespace Fauna;

internal abstract class LinqIntermediateExpr
{
    // The result type of the original C# expression.
    public abstract Type ExprType { get; }

    // return a subexpression that when executed returns a Query value
    public abstract Expression Build(ParameterExpression closure);

    internal virtual List<object> AsFragments() => new List<object> { this };

    public static LinqIntermediateExpr Join(Type ty, LinqIntermediateExpr a, string op, LinqIntermediateExpr b) =>
        a.Concat(ty, new Expr(ty, op)).Concat(ty, b);

    public abstract LinqIntermediateExpr Access(Type ty, string member);

    public LinqIntermediateExpr Concat(Type ty, string frag) => Concat(ty, new Expr(ty, frag));

    public LinqIntermediateExpr Concat(Type ty, LinqIntermediateExpr other)
    {
        var a = this.AsFragments();
        var b = other.AsFragments();
        var frags = new List<object>();

        // if the last fragment of a and b are strings, then concat them
        // together in the result.
        if (a.Last() is string astr && b.First() is string bstr)
        {
            frags.AddRange(a.SkipLast(1));
            frags.Add(astr + bstr);
            frags.AddRange(b.Skip(1));
        }
        else
        {
            frags.AddRange(a);
            frags.AddRange(b);
        }

        return new Expr(ty, frags);
    }

    // A literal C# value. We can eagerly transform these when processing the C# expr.
    internal class Constant : LinqIntermediateExpr
    {
        public object? Value { get; }
        public override Type ExprType { get; }

        public Constant(Type ty, object? val)
        {
            Value = val;
            ExprType = ty;
        }

        public override string ToString() => $"Constant({Value})";

        public override LinqIntermediateExpr Access(Type ty, string member)
        {
            var expr = Expression.PropertyOrField(Expression.Constant(Value), member);
            var newVal = Expression.Lambda(expr).Compile().DynamicInvoke();

            return new Constant(ty, newVal);
        }

        public override Expression Build(ParameterExpression closure)
        {
            var queryType = typeof(QueryVal<>).MakeGenericType(ExprType);
            var queryVal = Activator.CreateInstance(queryType, Value)!;

            return Expression.Constant(queryVal, queryType);
        }
    }

    // A closure reference, within the C# expr. We build up a call chain which
    // is invoked when generating the FQL query.
    internal class Closure : LinqIntermediateExpr
    {
        public Func<ParameterExpression, Expression> Transformation { get; }
        public override Type ExprType { get; }

        public Closure(Type ty, Func<ParameterExpression, Expression> tx)
        {
            Transformation = tx;
            ExprType = ty;
        }

        public Closure(Type ty) : this(ty, p => p) { }

        public override string ToString() =>
            $"Closure({Transformation(Expression.Parameter(typeof(object), "_"))})";

        public override LinqIntermediateExpr Access(Type ty, string member)
        {
            var tx = Transformation;
            return new Closure(ty, p => Expression.PropertyOrField(tx(p), member));
        }

        public override Expression Build(ParameterExpression closure)
        {
            var queryType = typeof(QueryVal<>).MakeGenericType(ExprType);
            var queryCtor = queryType.GetConstructor(new Type[] { ExprType })!;

            return Expression.New(queryCtor, Transformation(closure));
        }
    }

    // An expression which is translated to FQL
    internal class Expr : LinqIntermediateExpr
    {
        internal List<object> _fragments;

        public override Type ExprType { get; }

        public Expr(Type ty, List<object> fragments)
        {
            _fragments = fragments;
            ExprType = ty;
        }

        public Expr(Type ty, string fragment) : this(ty, new List<object> { fragment }) { }

        public override string ToString() => $"Expr({string.Join(", ", _fragments)})";

        internal override List<object> AsFragments() => _fragments;

        public override LinqIntermediateExpr Access(Type ty, string member)
        {
            // TODO(matt) translate the member name by reflecting on the type
            // of the callee in order to grab our codec dict.
            // TODO(matt) properly escape the field name.

            return Concat(ty, "." + member);
        }

        public override Expression Build(ParameterExpression closure)
        {
            Expression ToExpr(object frag) => frag switch
            {
                string l => Expression.Constant(new QueryLiteral(l)),
                LinqIntermediateExpr e => e.Build(closure),
                _ => throw new InvalidOperationException("Unreachable!")
            };

            var fragExprs = _fragments.Select(ToExpr).ToList();
            var fragArr = Expression.NewArrayInit(typeof(IQueryFragment), fragExprs);
            var queryType = typeof(QueryExpr);
            var argTypes = new Type[] { typeof(IList<IQueryFragment>) };
            var queryCtor = queryType.GetConstructor(argTypes)!;

            return Expression.New(queryCtor, fragArr);
        }
    }
}
