using System.Linq.Expressions;

namespace Fauna.Linq;

using IE = IntermediateExpr;

internal abstract class IntermediateExpr
{
    public static IE Exp(string fql) => new Expr(fql);
    public static IE Const(object? v) => new Constant(v);

    public static IE Field(IE callee, string f) => callee.Access(f);

    private static readonly IE _lp = new Expr("(");
    private static readonly IE _rp = new Expr(")");
    public static IE Parens(IE inner) => _lp.Concat(inner).Concat(_rp);
    public static IE Parens(IEnumerable<IE> inners) => Join(inners, _lp, ",", _rp);

    private static readonly IE _lb = new Expr("{");
    private static readonly IE _rb = new Expr("}");
    public static IE Block(IE inner) => _lb.Concat(inner).Concat("}");
    public static IE Block(IEnumerable<IE> inners) => Join(inners, _lb, ";", _rb);

    public static IE Op(IE a, string op, IE b) =>
        a.Concat(new Expr(op)).Concat(b);

    public static IE MethodCall(IE callee, string m) =>
        MethodCall(callee, m, new IE[] { });

    public static IE MethodCall(IE callee, string m, IE arg) =>
        MethodCall(callee, m, new IE[] { arg });

    public static IE MethodCall(IE callee, string m, IEnumerable<IE> args) =>
        Join(args, callee.Concat($".{m}("), ",", _rp);

    public static IE Join(IEnumerable<IE> ies, IE l, string sep, IE r)
    {
        IE ret = l;
        var init = true;
        foreach (var ie in ies)
        {
            if (init) init = false; else ret = ret.Concat(sep);
            ret = ret.Concat(ie);
        }
        ret = ret.Concat(r);
        return ret;
    }

    // return a subexpression that when executed returns a Query value
    public abstract Expression Build();

    internal virtual List<object> AsFragments() => new List<object> { this };

    public abstract IntermediateExpr Access(string member);

    public IntermediateExpr Concat(string frag) => Concat(new Expr(frag));

    public IntermediateExpr Concat(IntermediateExpr other)
    {
        var a = this.AsFragments();
        var b = other.AsFragments();
        var frags = new List<object>();

        // if the last fragment of a and the first of b are strings, then concat
        // them together in the result.
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

        return new Expr(frags);
    }

    // A literal C# value. We can eagerly transform these when processing the C# expr.
    internal class Constant : IntermediateExpr
    {
        public object? Value { get; }

        public Constant(object? val)
        {
            Value = val;
        }

        public override string ToString() => $"Constant({Value})";

        public override IntermediateExpr Access(string member)
        {
            var expr = Expression.PropertyOrField(Expression.Constant(Value), member);
            var newVal = Expression.Lambda(expr).Compile().DynamicInvoke();

            return new Constant(newVal);
        }

        public override Expression Build()
        {
            return Expression.Constant(new QueryVal(Value), typeof(QueryVal));
        }
    }

    // A closure reference, within the C# expr. We build up a call chain which
    // is invoked when generating the FQL query.
    internal class Closure : IntermediateExpr
    {
        Expression expr;

        public Closure(Expression e)
        {
            expr = e;
        }

        public override string ToString() => $"Closure({expr})";

        public override IntermediateExpr Access(string member)
        {
            var e = Expression.PropertyOrField(expr, member);
            return new Closure(e);
        }

        public override Expression Build()
        {
            var argTypes = new Type[] { typeof(object) };
            var queryCtor = typeof(QueryVal).GetConstructor(argTypes)!;
            return Expression.New(queryCtor, expr);
        }
    }

    // An expression which is translated to FQL
    internal class Expr : IntermediateExpr
    {
        internal List<object> _fragments;

        public Expr(List<object> fragments)
        {
            _fragments = fragments;
        }

        public Expr(string fragment) : this(new List<object> { fragment }) { }

        public override string ToString() => $"Expr({string.Join(", ", _fragments)})";

        internal override List<object> AsFragments() => _fragments;

        public override IntermediateExpr Access(string member)
        {
            // TODO(matt) translate the member name by reflecting on the type
            // of the callee in order to grab our codec dict.
            // TODO(matt) properly escape the field name.
            return Concat("." + member);
        }

        public override Expression Build()
        {
            Expression ToExpr(object frag) => frag switch
            {
                string l => Expression.Constant(new QueryLiteral(l)),
                IntermediateExpr e => e.Build(),
                _ => throw new InvalidOperationException("Unreachable!")
            };

            var fragExprs = _fragments.Select(ToExpr).ToList();
            var fragArr = Expression.NewArrayInit(typeof(IQueryFragment), fragExprs);
            var argTypes = new Type[] { typeof(IList<IQueryFragment>) };
            var queryCtor = typeof(QueryExpr).GetConstructor(argTypes)!;
            return Expression.New(queryCtor, fragArr);
        }
    }
}
