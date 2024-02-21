using Fauna.Types;
using System.Diagnostics.CodeAnalysis;

namespace Fauna.Linq;

public abstract class QuerySource : IQuerySource
{
    [AllowNull]
    internal DataContext Ctx { get; private protected set; }
    [AllowNull]
    internal Pipeline Pipeline { get; private protected set; }

    internal void SetContext(DataContext ctx)
    {
        Ctx = ctx;
    }

    internal void SetQuery<TElem>(Query query)
    {
        Pipeline = new Pipeline(PipelineMode.Query, query, typeof(TElem), false, null, null);
    }

    // DSL Helpers

    internal abstract TResult Execute<TResult>(Pipeline pl);

    internal abstract Task<TResult> ExecuteAsync<TResult>(Pipeline pl, CancellationToken cancel = default);
}

public partial class QuerySource<T> : QuerySource, IQuerySource<T>
{
    internal QuerySource(DataContext ctx, Pipeline pl)
    {
        Ctx = ctx;
        Pipeline = pl;
    }

    // Collection/Index DSLs are allowed to set _expr and _ctx in their own
    // constructors, so they use this base one.
    internal QuerySource() { }

    internal override TResult Execute<TResult>(Pipeline pl)
    {
        try
        {
            var res = ExecuteAsync<TResult>(pl);
            res.Wait();
            return res.Result;
        }
        catch (AggregateException ex)
        {
            throw ex.InnerExceptions.First();
        }
    }

    internal override Task<TResult> ExecuteAsync<TResult>(Pipeline pl, CancellationToken cancel = default) =>
        pl.GetExec(Ctx).Result<TResult>(queryOptions: null, cancel: cancel);

    public IAsyncEnumerable<Page<T>> PaginateAsync(QueryOptions? queryOptions = null, CancellationToken cancel = default)
    {
        var pe = Pipeline.GetExec(Ctx);
        return pe.PagedResult<T>(queryOptions, cancel);
    }

    public IAsyncEnumerable<T> ToAsyncEnumerable(CancellationToken cancel = default) =>
        PaginateAsync(cancel: cancel).FlattenAsync();

    public IEnumerable<T> ToEnumerable() => new QuerySourceEnumerable(this);
}
