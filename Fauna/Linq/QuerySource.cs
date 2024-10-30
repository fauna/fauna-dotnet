using System.Diagnostics.CodeAnalysis;
using Fauna.Core;
using Fauna.Types;
using Fauna.Util.Extensions;

namespace Fauna.Linq;

/// <summary>
/// An abstract class representing a QuerySource for LINQ-style queries.
/// </summary>
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

    /// <summary>
    /// Executes a query with pagination.
    /// </summary>
    /// <param name="queryOptions">Query options.</param>
    /// <param name="cancel">A cancellation token.</param>
    /// <returns>An async enumerable for page results.</returns>
    public IAsyncEnumerable<Page<T>> PaginateAsync(QueryOptions? queryOptions = null, CancellationToken cancel = default)
    {
        var pe = Pipeline.GetExec(Ctx);
        return pe.PagedResult<T>(queryOptions, cancel);
    }

    /// <summary>
    /// Executes a query asynchronously.
    /// </summary>
    /// <param name="cancel">A cancellation token.</param>
    /// <returns>A enumerable of results.</returns>
    public IAsyncEnumerable<T> ToAsyncEnumerable(CancellationToken cancel = default) =>
        PaginateAsync(cancel: cancel).FlattenAsync();

    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <returns>A enumerable of results.</returns>
    public IEnumerable<T> ToEnumerable() => new QuerySourceEnumerable(this);
}
