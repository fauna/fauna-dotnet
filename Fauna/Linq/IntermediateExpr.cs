using System.Linq.Expressions;

namespace Fauna.Linq;

using IE = IntermediateExpr;

internal abstract class IntermediateExpr
{
    public static IE Exp(string fql) => new Expr(fql);
    public static IE Const(object? v) => new Constant(v);

    public static IE Field(IE callee, string f) => callee.Access(f);

    private static readonly IE _larr = new Expr("[");
    private static readonly IE _rarr = new Expr("]");
    public static IE Array(IE inner) => _larr.Concat(inner).Concat(_rarr);
    public static IE Array(IEnumerable<IE> inners) => Join(inners, _larr, ",", _rarr);

    private static readonly IE _lparen = new Expr("(");
    private static readonly IE _rparen = new Expr(")");
    public static IE Parens(IE inner) => _lparen.Concat(inner).Concat(_rparen);
    public static IE Parens(IEnumerable<IE> inners) => Join(inners, _lparen, ",", _rparen);

    private static readonly IE _lbrace = new Expr("{");
    private static readonly IE _rbrace = new Expr("}");
    public static IE Block(IE inner) => _lbrace.Concat(inner).Concat("}");
    public static IE Block(IEnumerable<IE> inners) => Join(inners, _lbrace, ";", _rbrace);
    public static IE Obj(IE inner) => _lbrace.Concat(inner).Concat("}");
    public static IE Obj(IEnumerable<IE> inners) => Join(inners, _lbrace, ",", _rbrace);

    public static IE Op(IE a, string op, IE b) =>
        a.Concat(new Expr(op)).Concat(b);

    public static IE FnCall(string m) =>
        FnCall(m, new IE[] { });

    public static IE FnCall(string m, IE arg) =>
        FnCall(m, new IE[] { arg });

    public static IE FnCall(string m, IEnumerable<IE> args) =>
        Join(args, Exp($"{m}("), ",", _rparen);

    public static IE MethodCall(IE callee, string m) =>
        MethodCall(callee, m, new IE[] { });

    public static IE MethodCall(IE callee, string m, IE arg) =>
        MethodCall(callee, m, new IE[] { arg });

    public static IE MethodCall(IE callee, string m, IEnumerable<IE> args) =>
        Join(args, callee.Concat($".{m}("), ",", _rparen);

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
