using Fauna.Serialization;
using Fauna.Types;
using Fauna.Util;
using System.Linq.Expressions;

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
        var closures = ExpressionClosures.FindAll(expr);

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
    IDeserializer Deserializer);

internal static class PipelineClosureExtensions
{
    public static async Task<T> ResultAsync<T>(
        this PipelineClosure cls,
        QueryOptions? queryOpts)
    {
        var query = cls.Query;
        var deser = (IDeserializer<T>)cls.Deserializer;
        var qres = await cls.Ctx.QueryAsync(query, deser, queryOpts);

        return qres.Data;
    }

    public static async IAsyncEnumerable<Page<T>> PaginateAsync<T>(
        this PipelineClosure cls,
        QueryOptions? queryOpts)
    {
        var query = cls.Query;
        var deser = (PageDeserializer<T>)cls.Deserializer;
        var qres = cls.Ctx.PaginateAsyncInternal(query, deser, queryOpts);

        await foreach (var page in qres)
        {
            yield return page;
        }
    }
}
