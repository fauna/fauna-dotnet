using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Util;
using QH = Fauna.Linq.IntermediateQueryHelpers;

namespace Fauna.Linq;

public partial class QuerySource<T>
{
    private Query Query { get => Pipeline.Query; }
    private MappingContext MappingCtx { get => Ctx.MappingCtx; }
    private LookupTable Lookup { get => Ctx.LookupTable; }

    // Composition methods

    public IQuerySource<T> Distinct()
    {
        RequireQueryMode();
        return Chain<T>(q: QH.MethodCall(Query, "distinct"));
    }

    public IQuerySource<T> Order()
    {
        RequireQueryMode();
        return Chain<T>(q: QH.MethodCall(Query, "order"));
    }

    public IQuerySource<T> OrderBy<K>(Expression<Func<T, K>> keySelector)
    {
        RequireQueryMode();
        return Chain<T>(q: QH.MethodCall(Query, "order", SubQuery(keySelector)));
    }

    public IQuerySource<T> OrderByDescending<K>(Expression<Func<T, K>> keySelector)
    {
        RequireQueryMode();
        return Chain<T>(q: QH.MethodCall(Query, "order", QH.FnCall("desc", SubQuery(keySelector))));
    }

    public IQuerySource<T> OrderDescending()
    {
        RequireQueryMode();
        return Chain<T>(q: QH.MethodCall(Query, "order", QH.Expr("desc(x => x)")));
    }

    public IQuerySource<T> Reverse() =>
        Chain<T>(q: QH.MethodCall(Query, "reverse"));

    public IQuerySource<R> Select<R>(Expression<Func<T, R>> selector)
    {
        var pl = SelectCall(Query, selector);
        return new QuerySource<R>(Ctx, pl);
    }

    public IQuerySource<T> Skip(int count) =>
         Chain<T>(q: QH.MethodCall(Query, "drop", QH.Const(count)));

    public IQuerySource<T> Take(int count) =>
         Chain<T>(q: QH.MethodCall(Query, "take", QH.Const(count)));

    public IQuerySource<T> Where(Expression<Func<T, bool>> predicate) =>
        Chain<T>(q: WhereCall(Query, predicate));

    // Terminal result methods

    public bool All(Expression<Func<T, bool>> predicate) => Execute<bool>(AllImpl(predicate));
    public Task<bool> AllAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default) =>
        ExecuteAsync<bool>(AllImpl(predicate), cancel);
    private Pipeline AllImpl(Expression<Func<T, bool>> predicate)
    {
        RequireQueryMode("All");
        return CopyPipeline(
            mode: PipelineMode.Scalar,
            q: QH.MethodCall(Query, "every", SubQuery(predicate)),
            ety: typeof(bool));
    }

    public bool Any() => Execute<bool>(AnyImpl(null));
    public Task<bool> AnyAsync(CancellationToken cancel = default) =>
        ExecuteAsync<bool>(AnyImpl(null), cancel);
    public bool Any(Expression<Func<T, bool>> predicate) => Execute<bool>(AnyImpl(predicate));
    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default) =>
        ExecuteAsync<bool>(AnyImpl(predicate), cancel);
    private Pipeline AnyImpl(Expression<Func<T, bool>>? predicate) =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: QH.MethodCall(MaybeWhereCall(Query, predicate), "nonEmpty"),
            ety: typeof(bool));

    public int Count() => Execute<int>(CountImpl(null));
    public Task<int> CountAsync(CancellationToken cancel = default) =>
        ExecuteAsync<int>(CountImpl(null), cancel);
    public int Count(Expression<Func<T, bool>> predicate) => Execute<int>(CountImpl(predicate));
    public Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default) =>
        ExecuteAsync<int>(CountImpl(predicate), cancel);
    private Pipeline CountImpl(Expression<Func<T, bool>>? predicate) =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: QH.MethodCall(MaybeWhereCall(Query, predicate), "count"),
            ety: typeof(int));

    public T First() => Execute<T>(FirstImpl(null));
    public Task<T> FirstAsync(CancellationToken cancel = default) =>
        ExecuteAsync<T>(FirstImpl(null), cancel);
    public T First(Expression<Func<T, bool>> predicate) => Execute<T>(FirstImpl(predicate));
    public Task<T> FirstAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default) =>
        ExecuteAsync<T>(FirstImpl(predicate), cancel);
    private Pipeline FirstImpl(Expression<Func<T, bool>>? predicate) =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: QH.MethodCall(AbortIfEmpty(MaybeWhereCall(Query, predicate)), "first"));

    public T? FirstOrDefault() => Execute<T?>(FirstOrDefaultImpl(null));
    public Task<T?> FirstOrDefaultAsync(CancellationToken cancel = default) =>
        ExecuteAsync<T?>(FirstOrDefaultImpl(null), cancel);
    public T? FirstOrDefault(Expression<Func<T, bool>> predicate) => Execute<T?>(FirstOrDefaultImpl(predicate));
    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default) =>
        ExecuteAsync<T?>(FirstOrDefaultImpl(predicate), cancel);
    private Pipeline FirstOrDefaultImpl(Expression<Func<T, bool>>? predicate) =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: QH.MethodCall(MaybeWhereCall(Query, predicate), "first"),
            ety: typeof(T),
            enull: true);

    public T Last() => Execute<T>(LastImpl(null));
    public Task<T> LastAsync(CancellationToken cancel = default) =>
        ExecuteAsync<T>(LastImpl(null), cancel);
    public T Last(Expression<Func<T, bool>> predicate) => Execute<T>(LastImpl(predicate));
    public Task<T> LastAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default) =>
        ExecuteAsync<T>(LastImpl(predicate), cancel);
    private Pipeline LastImpl(Expression<Func<T, bool>>? predicate) =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: QH.MethodCall(AbortIfEmpty(MaybeWhereCall(Query, predicate)), "last"));

    public T? LastOrDefault() => Execute<T?>(LastOrDefaultImpl(null));
    public Task<T?> LastOrDefaultAsync(CancellationToken cancel = default) =>
        ExecuteAsync<T?>(LastOrDefaultImpl(null), cancel);
    public T? LastOrDefault(Expression<Func<T, bool>> predicate) => Execute<T?>(LastOrDefaultImpl(predicate));
    public Task<T?> LastOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default) =>
        ExecuteAsync<T?>(LastOrDefaultImpl(predicate), cancel);
    private Pipeline LastOrDefaultImpl(Expression<Func<T, bool>>? predicate) =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: QH.MethodCall(MaybeWhereCall(Query, predicate), "last"),
            ety: typeof(T),
            enull: true);

    public long LongCount() => Execute<long>(LongCountImpl(null));
    public Task<long> LongCountAsync(CancellationToken cancel = default) =>
        ExecuteAsync<long>(LongCountImpl(null), cancel);
    public long LongCount(Expression<Func<T, bool>> predicate) => Execute<long>(LongCountImpl(predicate));
    public Task<long> LongCountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default) =>
        ExecuteAsync<long>(LongCountImpl(predicate), cancel);
    private Pipeline LongCountImpl(Expression<Func<T, bool>>? predicate) =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: QH.MethodCall(MaybeWhereCall(Query, predicate), "count"),
            ety: typeof(long));

    private static readonly Query _maxReducer = QH.Expr("(a, b) => if (a >= b) a else b");

    public T Max() => Execute<T>(MaxImpl<T>(null));
    public Task<T> MaxAsync(CancellationToken cancel = default) =>
        ExecuteAsync<T>(MaxImpl<T>(null), cancel);
    public R Max<R>(Expression<Func<T, R>> selector) => Execute<R>(MaxImpl(selector));
    public Task<R> MaxAsync<R>(Expression<Func<T, R>> selector, CancellationToken cancel = default) =>
        ExecuteAsync<R>(MaxImpl(selector), cancel);
    private Pipeline MaxImpl<R>(Expression<Func<T, R>>? selector, [CallerMemberName] string callerName = "")
    {
        RequireQueryMode(callerName);
        return CopyPipeline(
            mode: PipelineMode.Scalar,
            q: QH.MethodCall(MaybeMap(AbortIfEmpty(Query), selector), "reduce", _maxReducer),
            ety: typeof(R));
    }

    private static readonly Query _minReducer = QH.Expr("(a, b) => if (a <= b) a else b");

    public T Min() => Execute<T>(MinImpl<T>(null));
    public Task<T> MinAsync(CancellationToken cancel = default) => ExecuteAsync<T>(MinImpl<T>(null), cancel);
    public R Min<R>(Expression<Func<T, R>> selector) => Execute<R>(MinImpl(selector));
    public Task<R> MinAsync<R>(Expression<Func<T, R>> selector, CancellationToken cancel = default) =>
        ExecuteAsync<R>(MinImpl(selector), cancel);
    private Pipeline MinImpl<R>(Expression<Func<T, R>>? selector, [CallerMemberName] string callerName = "")
    {
        RequireQueryMode(callerName);
        return CopyPipeline(
            mode: PipelineMode.Scalar,
            q: QH.MethodCall(MaybeMap(AbortIfEmpty(Query), selector), "reduce", _minReducer),
            ety: typeof(R));
    }

    public double Average(Expression<Func<T, double>> selector) => Execute<double>(AverageImpl(selector));
    public Task<double> AverageAsync(Expression<Func<T, double>> selector, CancellationToken cancel = default) =>
        ExecuteAsync<double>(AverageImpl(selector), cancel);

    private Pipeline AverageImpl<R>(Expression<Func<T, R>> selector)
    {
        RequireQueryMode("Average");

        return CopyPipeline(
            mode: PipelineMode.Scalar,
            q: QH.FnCall("Math.mean", QH.MethodCall(QH.MethodCall(AbortIfEmpty(Query), "map", SubQuery(selector)), "toArray")),
            ety: typeof(R));
    }

    public T Single() => Execute<T>(SingleImpl(null));
    public Task<T> SingleAsync(CancellationToken cancel = default) => ExecuteAsync<T>(SingleImpl(null), cancel);
    public T Single(Expression<Func<T, bool>> predicate) => Execute<T>(SingleImpl(predicate));
    public Task<T> SingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default) =>
        ExecuteAsync<T>(SingleImpl(predicate), cancel);
    private Pipeline SingleImpl(Expression<Func<T, bool>>? predicate) =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: QH.MethodCall(AbortIfEmpty(Singularize(MaybeWhereCall(Query, predicate))), "first"));

    public T SingleOrDefault() => Execute<T>(SingleOrDefaultImpl(null));
    public Task<T> SingleOrDefaultAsync(CancellationToken cancel = default) => ExecuteAsync<T>(SingleOrDefaultImpl(null), cancel);
    public T SingleOrDefault(Expression<Func<T, bool>> predicate) => Execute<T>(SingleOrDefaultImpl(predicate));
    public Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default) =>
        ExecuteAsync<T>(SingleOrDefaultImpl(predicate), cancel);
    private Pipeline SingleOrDefaultImpl(Expression<Func<T, bool>>? predicate) =>
        CopyPipeline(
            mode: PipelineMode.Scalar,
            q: QH.MethodCall(Singularize(MaybeWhereCall(Query, predicate)), "first"),
            ety: typeof(T),
            enull: true);

    private static readonly Query _sumReducer = QH.Expr("(a, b) => a + b");

    public int Sum(Expression<Func<T, int>> selector) => Execute<int>(SumImpl<int>(selector));
    public Task<int> SumAsync(Expression<Func<T, int>> selector, CancellationToken cancel = default) =>
        ExecuteAsync<int>(SumImpl<int>(selector), cancel);
    public long Sum(Expression<Func<T, long>> selector) => Execute<long>(SumImpl<long>(selector));
    public Task<long> SumAsync(Expression<Func<T, long>> selector, CancellationToken cancel = default) =>
        ExecuteAsync<long>(SumImpl<long>(selector), cancel);
    public double Sum(Expression<Func<T, double>> selector) => Execute<double>(SumImpl<double>(selector));
    public Task<double> SumAsync(Expression<Func<T, double>> selector, CancellationToken cancel = default) =>
        ExecuteAsync<double>(SumImpl<double>(selector), cancel);
    private Pipeline SumImpl<R>(Expression<Func<T, R>> selector)
    {
        RequireQueryMode("Sum");
        var seed = (typeof(R) == typeof(int) || typeof(R) == typeof(long)) ?
            QH.Expr("0") :
            QH.Expr("0.0");
        var mapped = QH.MethodCall(Query, "map", SubQuery(selector));
        return CopyPipeline(
            mode: PipelineMode.Scalar,
            q: QH.MethodCall(mapped, "fold", seed, _sumReducer),
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

    private R Execute<R>(Pipeline pl)
    {
        try
        {
            var res = ExecuteAsync<R>(pl);
            res.Wait();
            return res.Result;
        }
        catch (AggregateException ex)
        {
            throw TranslateException(ex.InnerExceptions.First());
        }
    }

    private async Task<R> ExecuteAsync<R>(Pipeline pl, CancellationToken cancel = default)
    {
        try
        {
            return await pl.GetExec(Ctx).Result<R>(queryOptions: null, cancel: cancel);
        }
        catch (AggregateException ex)
        {
            throw TranslateException(ex.InnerExceptions.First());
        }
    }

    private QuerySource<R> Chain<R>(
        PipelineMode? mode = null,
        Query? q = null,
        ISerializer? ser = null,
        Type? ety = null,
        bool enull = false,
        LambdaExpression? proj = null) =>
        new QuerySource<R>(Ctx, CopyPipeline(mode, q, ser, ety, enull, proj));

    private Pipeline CopyPipeline(
        PipelineMode? mode = null,
        Query? q = null,
        ISerializer? ser = null,
        Type? ety = null,
        bool enull = false,
        LambdaExpression? proj = null)
    {
        if (ser is not null) Debug.Assert(ety is not null);

        var mode0 = mode ?? Pipeline.Mode;
        var q0 = q ?? Pipeline.Query;

        // if ety is not null, reset ser and proj if not provided.
        var (ety0, enull0, ser0, proj0) = ety is not null ?
            (ety, enull, ser, proj) :
            (Pipeline.ElemType,
             Pipeline.ElemNullable,
             Pipeline.ElemSerializer,
             proj ?? Pipeline.ProjectExpr);

        return new Pipeline(mode0, q0, ety0, enull0, ser0, proj0);
    }

    // There is a bug in abort data deserialization if the abort
    // value is a string. Work around it by using an array.
    // FIXME(matt) remove workaround and use a string
    private Query AbortIfEmpty(Query setq) =>
        QH.Expr(@"({ let s = (").Concat(setq).Concat(@")
            if (s.isEmpty()) abort(['empty'])
            s
        })");

    private Query Singularize(Query setq) =>
        QH.Expr(@"({
          let s = (").Concat(setq).Concat(@")
          let s = if (s isa Set) s.toArray() else s
          if (s isa Array) {
            if (s.length > 1) abort(['not single'])
            s.take(1)
          } else {
            [s]
          }
        })");

    private Exception TranslateException(Exception ex) =>
        ex switch
        {
            AbortException aex =>
                aex.GetData<List<string>>()?.First() switch
                {
                    "empty" => new InvalidOperationException("Empty set"),
                    "not single" => new InvalidOperationException("Set contains more than one element"),
                    _ => aex,
                },
            _ => ex
        };

    private Query MaybeWhereCall(Query callee, Expression? predicate, [CallerMemberName] string callerName = "") =>
        predicate is null ? callee : WhereCall(callee, predicate, callerName);

    private Query MaybeMap(Query setq, Expression? selector) =>
        selector is null ? setq : QH.MethodCall(setq, "map", SubQuery(selector));

    private Query SubQuery(Expression expr) =>
        new SubQuerySwitch(Ctx.LookupTable).Apply(expr);

    private Query WhereCall(Query callee, Expression predicate, [CallerMemberName] string callerName = "")
    {
        RequireQueryMode(callerName);
        return QH.MethodCall(callee, "where", SubQuery(predicate));
    }

    private Pipeline SelectCall(Query callee, Expression proj, [CallerMemberName] string callerName = "")
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

            return CopyPipeline(
                q: QH.MethodCall(callee, "map", QH.Expr($".{field.Name}")),
                ser: field.Serializer,
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
            var accs = fields.Select(f => QH.Expr($"x.{f.Name}"));
            var pquery = QH.Expr("x => ").Concat(QH.Array(accs));

            // projected field deserializer
            var deser = new ProjectionDeserializer(fields.Select(f => f.Serializer));
            var ety = typeof(object?[]);

            // build mapping lambda expression
            var pparam = Expression.Parameter(typeof(object?[]), "x");
            var rewriter = new ProjectionRewriteVisitor(lparam, accesses, pparam);
            var pbody = rewriter.Visit(lambda.Body);
            var plambda = Expression.Lambda(pbody, pparam);

            return CopyPipeline(
                q: QH.MethodCall(callee, "map", pquery),
                mode: PipelineMode.Project,
                ser: deser,
                ety: ety,
                proj: plambda);
        }
    }
}
