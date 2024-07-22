namespace Fauna.Linq;

internal static class IntermediateQueryHelpers
{
    public static QueryExpr Expr(string fql) => new(new List<IQueryFragment> { new QueryLiteral(fql) });

    public static QueryVal Const(object? v) => new(v);

    private static readonly Query _larr = Expr("[");
    private static readonly Query _rarr = Expr("]");
    public static Query Array(Query inner) => _larr.Concat(inner).Concat(_rarr);
    public static Query Array(IEnumerable<Query> inners) => Join(inners, _larr, ",", _rarr);

    private static readonly Query _lparen = Expr("(");
    private static readonly Query _rparen = Expr(")");
    public static Query Parens(Query inner) => _lparen.Concat(inner).Concat(_rparen);
    public static Query Parens(IEnumerable<Query> inners) => Join(inners, _lparen, ",", _rparen);

    private static readonly Query _lbrace = Expr("{");
    private static readonly Query _rbrace = Expr("}");
    public static Query Block(Query inner) => _lbrace.Concat(inner).Concat("}");
    public static Query Block(IEnumerable<Query> inners) => Join(inners, _lbrace, ";", _rbrace);
    public static Query Obj(Query inner) => _lbrace.Concat(inner).Concat("}");
    public static Query Obj(IEnumerable<Query> inners) => Join(inners, _lbrace, ",", _rbrace);

    public static Query Op(Query a, string op, Query b) =>
        a.Concat(Expr(op)).Concat(b);

    public static Query FieldAccess(Query callee, string f) =>
        callee.Concat($".{f}");

    public static Query FnCall(string m) =>
        Expr($"{m}()");

    public static Query FnCall(string m, Query arg) =>
        Expr($"{m}(").Concat(arg).Concat(_rparen);

    public static Query FnCall(string m, IEnumerable<Query> args) =>
        Join(args, Expr($"{m}("), ",", _rparen);

    public static Query MethodCall(Query callee, string m) =>
        callee.Concat($".{m}()");

    public static Query MethodCall(Query callee, string m, Query a1) =>
        callee.Concat($".{m}(").Concat(a1).Concat(_rparen);

    public static Query MethodCall(Query callee, string m, Query a1, Query a2) =>
        callee.Concat($".{m}(").Concat(a1).Concat(",").Concat(a2).Concat(_rparen);

    public static Query MethodCall(Query callee, string m, IEnumerable<Query> args) =>
        Join(args, callee.Concat($".{m}("), ",", _rparen);

    public static Query Join(IEnumerable<Query> ies, Query l, string sep, Query r)
    {
        Query ret = l;
        var init = true;
        foreach (var ie in ies)
        {
            if (init) init = false; else ret = ret.Concat(sep);
            ret = ret.Concat(ie);
        }
        ret = ret.Concat(r);
        return ret;
    }

    public static Query CollectionAll(DataContext.ICollection col) =>
        MethodCall(Expr(col.Name), "all");

    public static Query CollectionIndex(DataContext.IIndex idx) =>
        MethodCall(Expr(idx.Collection.Name), idx.Name, idx.Args.Select(Const));

    public static Query Function(string name, object[] args) =>
        FnCall(name, args.Select(Const));

    public static QueryExpr Concat(this Query q1, string str)
    {
        var frags = new List<IQueryFragment>();

        if (q1 is QueryExpr e1)
        {
            if (e1.Fragments.Last() is QueryLiteral l1)
            {
                frags.AddRange(e1.Fragments.SkipLast(1));
                frags.Add(new QueryLiteral(l1.Unwrap + str));
            }
            else
            {
                frags.AddRange(e1.Fragments);
                frags.Add(new QueryLiteral(str));
            }
        }
        else
        {
            frags.Add(q1);
            frags.Add(new QueryLiteral(str));
        }

        return new QueryExpr(frags);
    }

    public static QueryExpr Concat(this Query q1, Query q2)
    {
        var frags = new List<IQueryFragment>();

        if (q1 is QueryExpr e1)
        {
            if (q2 is QueryExpr e2)
            {
                if (e1.Fragments.Last() is QueryLiteral l1 &&
                    e2.Fragments.First() is QueryLiteral l2)
                {
                    frags.AddRange(e1.Fragments.SkipLast(1));
                    frags.Add(new QueryLiteral(l1.Unwrap + l2.Unwrap));
                    frags.AddRange(e2.Fragments.Skip(1));
                }
                else
                {
                    frags.AddRange(e1.Fragments);
                    frags.AddRange(e2.Fragments);
                }
            }
            else
            {
                frags.AddRange(e1.Fragments);
                frags.Add(q2);
            }
        }
        else
        {
            if (q2 is QueryExpr e2)
            {
                frags.Add(q1);
                frags.AddRange(e2.Fragments);
            }
            else
            {
                frags.Add(q1);
                frags.Add(q2);
            }
        }

        return new QueryExpr(frags);
    }
}
