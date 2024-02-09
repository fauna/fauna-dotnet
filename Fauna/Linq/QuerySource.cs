using Fauna.Types;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Fauna.Linq;

public abstract class QuerySource : IQuerySource
{
    [AllowNull]
    internal DataContext Ctx { get; private protected set; }
    [AllowNull]
    internal Expression Expr { get; private protected set; }

    internal void SetContext(DataContext ctx)
    {
        Ctx = ctx;
    }

    // DSL Helpers

    internal abstract TResult Execute<TResult>(Expression expression);
}

public class QuerySource<T> : QuerySource, IQuerySource<T>
{
    public QuerySource(Expression expr, DataContext ctx)
    {
        Expr = expr;
        Ctx = ctx;
    }

    // Collection/Index DSLs are allowed to set _expr and _ctx in their own
    // constructors, so they use this base one.
    internal QuerySource() { }

    internal override TResult Execute<TResult>(Expression expression)
    {
        var pl = Ctx.PipelineCache.Get(Ctx, expression);
        var res = pl.Result<TResult>(queryOptions: null);
        res.Wait();
        return res.Result;
    }

    public IAsyncEnumerable<Page<T>> PaginateAsync(QueryOptions? queryOptions = null)
    {
        var pl = Ctx.PipelineCache.Get(Ctx, Expr);
        return pl.PagedResult<T>(queryOptions);
    }

    public IAsyncEnumerable<T> ToAsyncEnumerable() => PaginateAsync().FlattenAsync();

    public IEnumerable<T> ToEnumerable() => new QuerySourceEnumerable(this);

    public record struct QuerySourceEnumerable(QuerySource<T> Source) : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            var pe = Source.PaginateAsync().GetAsyncEnumerator();
            try
            {
                var mv = pe.MoveNextAsync().AsTask();
                mv.Wait();
                while (mv.Result)
                {
                    var page = pe.Current;

                    foreach (var e in page.Data)
                    {
                        yield return e;
                    }

                    mv = pe.MoveNextAsync().AsTask();
                    mv.Wait();
                }
            }
            finally { pe.DisposeAsync(); }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
