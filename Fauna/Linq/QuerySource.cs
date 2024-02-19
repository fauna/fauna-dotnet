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
        Pipeline = new Pipeline(PipelineMode.Query, query, typeof(TElem), null, null);
    }

    // DSL Helpers

    internal abstract TResult Execute<TResult>(Pipeline pl);

    internal abstract Task<TResult> ExecuteAsync<TResult>(Pipeline pl);
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
        var res = ExecuteAsync<TResult>(pl);
        res.Wait();
        return res.Result;
    }

    internal override Task<TResult> ExecuteAsync<TResult>(Pipeline pl) =>
        pl.GetExec(Ctx).Result<TResult>(queryOptions: null);

    public IAsyncEnumerable<Page<T>> PaginateAsync(QueryOptions? queryOptions = null)
    {
        var pe = Pipeline.GetExec(Ctx);
        return pe.PagedResult<T>(queryOptions);
    }

    public IAsyncEnumerable<T> ToAsyncEnumerable() => PaginateAsync().FlattenAsync();

    public IEnumerable<T> ToEnumerable() => new QuerySourceEnumerable(this);
}
