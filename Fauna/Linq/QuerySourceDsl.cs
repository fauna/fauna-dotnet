using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Util;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;
using IE = Fauna.Linq.IntermediateExpr;

namespace Fauna.Linq;

public partial class QuerySource<T>
{
    private IE Query { get => Pipeline.Query; }
    private MappingContext MappingCtx { get => Ctx.MappingCtx; }
    private LookupTable Lookup { get => Ctx.LookupTable; }

    // Composition methods

    public IQuerySource<T> Distinct()
    {
        RequireQueryMode();
        return Chain<T>(q: IE.MethodCall(Query, "distinct"));
    }

    public IQuerySource<T> Order()
    {
        RequireQueryMode();
        return Chain<T>(q: IE.MethodCall(Query, "order"));
    }

    public IQuerySource<T> OrderBy<K>(Expression<Func<T, K>> keySelector)
    {
        RequireQueryMode();
        return Chain<T>(q: IE.MethodCall(Query, "order", SubQuery(keySelector)));
    }

    public IQuerySource<T> OrderByDescending<K>(Expression<Func<T, K>> keySelector)
    {
        RequireQueryMode();
        return Chain<T>(q: IE.MethodCall(Query, "order", IE.FnCall("desc", SubQuery(keySelector))));
    }

    public IQuerySource<T> OrderDescending()
    {
        RequireQueryMode();
        return Chain<T>(q: IE.MethodCall(Query, "order", IE.Exp("desc(x => x)")));
    }

    public IQuerySource<T> Reverse() =>
        Chain<T>(q: IE.MethodCall(Query, "reverse"));

    public IQuerySource<R> Select<R>(Expression<Func<T, R>> selector)
    {
        var pl = SelectCall(Query, selector);
        return new QuerySource<R>(Ctx, pl);
    }

    public IQuerySource<T> Skip(int count) =>
         Chain<T>(q: IE.MethodCall(Query, "drop", IE.Const(count)));

    public IQuerySource<T> Take(int count) =>
         Chain<T>(q: IE.MethodCall(Query, "take", IE.Const(count)));

    public IQuerySource<T> Where(Expression<Func<T, bool>> predicate) =>
        Chain<T>(q: WhereCall(Query, predicate));

    // Terminal result methods

    public bool All(Expression<Func<T, bool>> predicate) =>
        Execute<bool>(AllImpl(predicate));
    public Task<bool> AllAsync(Expression<Func<T, bool>> predicate) =>
        ExecuteAsync<bool>(AllImpl(predicate));
    private Pipeline AllImpl(Expression<Func<T, bool>> predicate)
    {
        RequireQueryMode("All");
        return CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(Query, "every", SubQuery(predicate)),
            ety: typeof(bool));
    }

    public bool Any() => Execute<bool>(AnyImpl());
    public Task<bool> AnyAsync() => ExecuteAsync<bool>(AnyImpl());
    private Pipeline AnyImpl() =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(Query, "nonEmpty"),
            ety: typeof(bool));

    public bool Any(Expression<Func<T, bool>> predicate) =>
        Execute<bool>(AnyImpl(predicate));
    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) =>
        ExecuteAsync<bool>(AnyImpl(predicate));
    private Pipeline AnyImpl(Expression<Func<T, bool>> predicate) =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(WhereCall(Query, predicate), "nonEmpty"),
            ety: typeof(bool));

    public int Count() => Execute<int>(CountImpl());
    public Task<int> CountAsync() => ExecuteAsync<int>(CountImpl());
    private Pipeline CountImpl() =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(Query, "count"),
            ety: typeof(int));

    public int Count(Expression<Func<T, bool>> predicate) =>
        Execute<int>(CountImpl(predicate));
    public Task<int> CountAsync(Expression<Func<T, bool>> predicate) =>
        ExecuteAsync<int>(CountImpl(predicate));
    private Pipeline CountImpl(Expression<Func<T, bool>> predicate) =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(WhereCall(Query, predicate), "count"),
            ety: typeof(int));

    public T First() => Execute<T>(FirstImpl());
    public Task<T> FirstAsync() => ExecuteAsync<T>(FirstImpl());
    private Pipeline FirstImpl() =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(Query, "first"));

    public T First(Expression<Func<T, bool>> predicate) =>
        Execute<T>(FirstImpl(predicate));
    public Task<T> FirstAsync(Expression<Func<T, bool>> predicate) =>
        ExecuteAsync<T>(FirstImpl(predicate));
    private Pipeline FirstImpl(Expression<Func<T, bool>> predicate) =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(WhereCall(Query, predicate), "first"));

    public T FirstOrDefault() => Execute<T>(FirstOrDefaultImpl());
    public Task<T> FirstOrDefaultAsync() => ExecuteAsync<T>(FirstOrDefaultImpl());
    private Pipeline FirstOrDefaultImpl() =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(Query, "first"));

    public T FirstOrDefault(Expression<Func<T, bool>> predicate) =>
        Execute<T>(FirstOrDefaultImpl(predicate));
    public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
        ExecuteAsync<T>(FirstOrDefaultImpl(predicate));
    private Pipeline FirstOrDefaultImpl(Expression<Func<T, bool>> predicate) =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(WhereCall(Query, predicate), "first"));

    public T Last() => Execute<T>(LastImpl());
    public Task<T> LastAsync() => ExecuteAsync<T>(LastImpl());
    private Pipeline LastImpl() =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(Query, "last"));

    public T Last(Expression<Func<T, bool>> predicate) =>
        Execute<T>(LastImpl(predicate));
    public Task<T> LastAsync(Expression<Func<T, bool>> predicate) =>
        ExecuteAsync<T>(LastImpl(predicate));
    private Pipeline LastImpl(Expression<Func<T, bool>> predicate) =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(WhereCall(Query, predicate), "last"));

    public T LastOrDefault() => Execute<T>(LastOrDefaultImpl());
    public Task<T> LastOrDefaultAsync() => ExecuteAsync<T>(LastOrDefaultImpl());
    private Pipeline LastOrDefaultImpl() =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(Query, "last"));

    public T LastOrDefault(Expression<Func<T, bool>> predicate) =>
        Execute<T>(LastOrDefaultImpl(predicate));
    public Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
        ExecuteAsync<T>(LastOrDefaultImpl(predicate));
    private Pipeline LastOrDefaultImpl(Expression<Func<T, bool>> predicate) =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(WhereCall(Query, predicate), "last"));

    public long LongCount() => Execute<long>(LongCountImpl());
    public Task<long> LongCountAsync() => ExecuteAsync<long>(LongCountImpl());
    private Pipeline LongCountImpl() =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(Query, "count"),
            ety: typeof(long));

    public long LongCount(Expression<Func<T, bool>> predicate) =>
        Execute<long>(LongCountImpl(predicate));
    public Task<long> LongCountAsync(Expression<Func<T, bool>> predicate) =>
        ExecuteAsync<long>(LongCountImpl(predicate));
    private Pipeline LongCountImpl(Expression<Func<T, bool>> predicate) =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(WhereCall(Query, predicate), "count"),
            ety: typeof(long));

    private static readonly IE _maxReducer = IE.Exp("(a, b) => if (a >= b) a else b");

    public T Max() => Execute<T>(MaxImpl());
    public Task<T> MaxAsync() => ExecuteAsync<T>(MaxImpl());
    private Pipeline MaxImpl() =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(Query, "reduce", _maxReducer));

    public R Max<R>(Expression<Func<T, R>> selector) => Execute<R>(MaxImpl(selector));
    public Task<R> MaxAsync<R>(Expression<Func<T, R>> selector) => ExecuteAsync<R>(MaxImpl(selector));
    private Pipeline MaxImpl<R>(Expression<Func<T, R>> selector)
    {
        RequireQueryMode("Max");
        return CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(IE.MethodCall(Query, "map", SubQuery(selector)), "reduce", _maxReducer),
            ety: typeof(R));
    }

    private static readonly IE _minReducer = IE.Exp("(a, b) => if (a <= b) a else b");

    public T Min() => Execute<T>(MinImpl());
    public Task<T> MinAsync() => ExecuteAsync<T>(MinImpl());
    private Pipeline MinImpl() =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(Query, "reduce", _minReducer));

    public R Min<R>(Expression<Func<T, R>> selector) => Execute<R>(MinImpl(selector));
    public Task<R> MinAsync<R>(Expression<Func<T, R>> selector) => ExecuteAsync<R>(MinImpl(selector));
    private Pipeline MinImpl<R>(Expression<Func<T, R>> selector)
    {
        RequireQueryMode("Min");
        return CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(IE.MethodCall(Query, "map", SubQuery(selector)), "reduce", _minReducer),
            ety: typeof(R));
    }

    private static readonly IE _sumReducer = IE.Exp("(a, b) => a + b");

    public int Sum(Expression<Func<T, int>> selector) => Execute<int>(SumImpl<int>(selector));
    public Task<int> SumAsync(Expression<Func<T, int>> selector) => ExecuteAsync<int>(SumImpl<int>(selector));
    public long Sum(Expression<Func<T, long>> selector) => Execute<long>(SumImpl<long>(selector));
    public Task<long> SumAsync(Expression<Func<T, long>> selector) => ExecuteAsync<long>(SumImpl<long>(selector));
    public float Sum(Expression<Func<T, float>> selector) => Execute<float>(SumImpl<float>(selector));
    public Task<float> SumAsync(Expression<Func<T, float>> selector) => ExecuteAsync<float>(SumImpl<float>(selector));
    public double Sum(Expression<Func<T, double>> selector) => Execute<double>(SumImpl<double>(selector));
    public Task<double> SumAsync(Expression<Func<T, double>> selector) => ExecuteAsync<double>(SumImpl<double>(selector));
    private Pipeline SumImpl<R>(Expression<Func<T, R>> selector)
    {
        RequireQueryMode("Sum");
        var seed = (typeof(R) == typeof(int) || typeof(R) == typeof(long)) ?
            IE.Exp("0") :
            IE.Exp("0.0");
        var mapped = IE.MethodCall(Query, "map", SubQuery(selector));
        return CopyPipeline(
            mode: PipelineMode.Scalar,
            q: IE.MethodCall(mapped, "fold", seed, _sumReducer),
            ety: typeof(R));
    }

    // helpers

    private void RequireQueryMode([CallerMemberName] string callerName = "")
    {
        if (Pipeline.Mode != PipelineMode.Query)
        {
            throw IQuerySource.Fail(
                callerName,
                $"Query is not pure: Earlier `Select` could not be translated to pure FQL.");
        }
    }

    private QuerySource<R> Chain<R>(
        PipelineMode? mode = null,
        IntermediateExpr? q = null,
        IDeserializer? deser = null,
        Type? ety = null,
        LambdaExpression? proj = null) =>
        new QuerySource<R>(Ctx, CopyPipeline(mode, q, deser, ety, proj));

    private Pipeline CopyPipeline(
        PipelineMode? mode = null,
        IntermediateExpr? q = null,
        IDeserializer? deser = null,
        Type? ety = null,
        LambdaExpression? proj = null)
    {
        if (deser is not null) Debug.Assert(ety is not null);

        var mode0 = mode ?? Pipeline.Mode;
        var q0 = q ?? Pipeline.Query;

        // if ety is not null, reset deser and proj if not provided.
        var (ety0, deser0, proj0) = ety is not null ?
            (ety, deser, proj) :
            (Pipeline.ElemType, Pipeline.ElemDeserializer, proj ?? Pipeline.ProjectExpr);

        return new Pipeline(mode0, q0, ety0, deser0, proj0);
    }

    private IE SubQuery(Expression expr) =>
        new SubQuerySwitch(Ctx.LookupTable).Apply(expr);

    private IE WhereCall(IE callee, Expression predicate, [CallerMemberName] string callerName = "")
    {
        RequireQueryMode(callerName);
        return IE.MethodCall(callee, "where", SubQuery(predicate));
    }

    private Pipeline SelectCall(IE callee, Expression proj, [CallerMemberName] string callerName = "")
    {
        var lambda = Expressions.UnwrapLambda(proj);
        Debug.Assert(lambda is not null, $"lambda is {proj.NodeType}");
        Debug.Assert(lambda.Parameters.Count() == 1);

        // there is already a projection wired up, so tack on to its mapping lambda
        if (Pipeline.Mode == PipelineMode.Project)
        {
            Debug.Assert(Pipeline.ProjectExpr is not null);
            var prev = Pipeline.ProjectExpr;
            var pbody = Expression.Invoke(lambda, new Expression[] { prev.Body });
            var plambda = Expression.Lambda(pbody, prev.Parameters);

            return CopyPipeline(proj: plambda);
        }

        Debug.Assert(Pipeline.Mode == PipelineMode.Query);

        var lparam = lambda.Parameters.First()!;
        var analysis = new ProjectionAnalysisVisitor(MappingCtx, lparam);
        analysis.Visit(lambda.Body);

        // select is a simple field access which we can translate directly to FQL.
        // TODO(matt) translate more cases to pure FQL
        if (lambda.Body is MemberExpression mexpr && mexpr.Expression == lparam)
        {
            Debug.Assert(!analysis.Escapes);
            var info = MappingCtx.GetInfo(lparam.Type);
            var access = analysis.Accesses.First();
            var field = Lookup.FieldLookup(access, lparam);
            Debug.Assert(field is not null);

            var pquery = IE.Exp("x => ").Concat(IE.Exp("x").Access(field.Name));
            return CopyPipeline(
                q: IE.MethodCall(callee, "map", pquery),
                deser: field.Deserializer,
                ety: field.Type);
        }

        if (analysis.Escapes)
        {
            return CopyPipeline(mode: PipelineMode.Project, proj: lambda);
        }
        else
        {
            var accesses = analysis.Accesses.OrderBy(f => f.Name).ToArray();
            var fields = accesses.Select(a => Lookup.FieldLookup(a, lparam)!);

            // projection query fragment
            var accs = fields.Select(f => IE.Exp("x").Access(f.Name));
            var pquery = IE.Exp("x => ").Concat(IE.Array(accs));

            // projected field deserializer
            var deser = new ProjectionDeserializer(fields.Select(f => f.Deserializer));
            var ety = typeof(object?[]);

            // build mapping lambda expression
            var pparam = Expression.Parameter(typeof(object?[]), "x");
            var rewriter = new ProjectionRewriteVisitor(lparam, accesses, pparam);
            var pbody = rewriter.Visit(lambda.Body);
            var plambda = Expression.Lambda(pbody, pparam);

            return CopyPipeline(
                q: IE.MethodCall(callee, "map", pquery),
                mode: PipelineMode.Project,
                deser: deser,
                ety: ety,
                proj: plambda);
        }
    }
}
