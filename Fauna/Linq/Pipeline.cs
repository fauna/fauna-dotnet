using Fauna.Serialization;
using Fauna.Types;
using Fauna.Util;
using System.Linq.Expressions;
using System.Reflection;

namespace Fauna.Linq;

internal readonly struct PipelineCache
{
    private readonly Dictionary<Expression, Pipeline> _cache;

    public PipelineCache()
    {
        _cache = new(new ExpressionComparer());
    }

    public PipelineClosure Get(DataContext ctx, Expression expr)
    {
        var closures = Expressions.FindAllClosures(expr);

        Pipeline pl;
        lock (_cache)
        {
            if (!_cache.TryGetValue(expr, out pl))
            {
                var builder = new PipelineBuilder(ctx, closures, expr);
                pl = builder.Build();
                _cache[expr] = pl;
            }
        }

        return pl.GetClosure(ctx, closures);
    }
}

internal readonly record struct Pipeline(
    Func<object[], Query> GetQuery,
    IDeserializer Deserializer)
{
    public PipelineClosure GetClosure(DataContext ctx, object[] vars) =>
        new PipelineClosure(ctx, GetQuery(vars), Deserializer);
}

internal readonly record struct PipelineClosure(
    DataContext Ctx,
    Query Query,
    IDeserializer Deserializer)
{
    private static readonly MethodInfo _resultAsyncMethod =
        typeof(PipelineClosure).GetMethod(nameof(ResultAsync), 1, new Type[] { typeof(QueryOptions) })!;

    public async Task<TResult> ResultAsync<TResult>(QueryOptions? queryOptions)
    {
        var deser = (IDeserializer<TResult>)Deserializer;
        var qres = await Ctx.QueryAsync<TResult>(Query, deser, queryOptions);
        return qres.Data;
    }

    public async Task<object?> ResultAsync(Type tresult, QueryOptions? queryOptions)
    {
        var ram = _resultAsyncMethod.MakeGenericMethod(new Type[] { tresult });
        var qtask = ram.Invoke(this, new[] { queryOptions });
        await (Task)qtask!;

        var field = qtask.GetType().GetProperty("Result", BindingFlags.Public | BindingFlags.Instance)!;
        return field.GetValue(qtask);
    }

    public async IAsyncEnumerable<Page<TResult>> PaginateAsync<TResult>(QueryOptions? queryOptions)
    {
        var deser = (PageDeserializer)Deserializer;
        var deser2 = new PageDeserializer<TResult>((IDeserializer<TResult>)deser.Elem);
        var qres = Ctx.PaginateAsyncInternal(Query, deser2, queryOptions);

        await foreach (var page in qres)
        {
            yield return page;
        }
    }
}
