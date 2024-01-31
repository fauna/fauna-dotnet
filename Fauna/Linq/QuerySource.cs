using Fauna.Types;
using Fauna.Serialization;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Fauna.Linq;

public abstract class QuerySource
{
    internal abstract void SetContext(DataContext ctx);
}

public class QuerySource<T> : QuerySource, IQueryable<T>, IQueryProvider
{
    [AllowNull]
    internal Expression _expr;
    [AllowNull]
    internal DataContext _ctx;

    public QuerySource(Expression expr, DataContext ctx)
    {
        _expr = expr;
        _ctx = ctx;
    }

    // Collection/Index DSLs are allowed to set _expr and _ctx in their own
    // constructors, so they use this base one.
    internal QuerySource() { }

    internal override void SetContext(DataContext ctx)
    {
        _ctx = ctx;
    }

    public IAsyncEnumerable<Page<T>> PaginateAsync(QueryOptions? queryOptions = null)
    {
        var pl = _ctx.PipelineCache.Get(_ctx, _expr);

        if (pl.Deserializer is PageDeserializer<T> pdeser)
        {
            return pl.PaginateAsync<T>(queryOptions);
        }

        throw new ArgumentException("Query does not result in an enumerable set");
    }

    public IAsyncEnumerable<T> AsAsyncEnumerable() => PaginateAsync().FlattenAsync();

    #region IQueryable members

    Type IQueryable.ElementType => typeof(T);
    Expression IQueryable.Expression => _expr;
    IQueryProvider IQueryable.Provider => this;

    #endregion

    #region IQueryProvider members

    IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
    {
        if (expression is null) throw new ArgumentNullException(nameof(expression));
        return new QuerySource<TElement>(expression, _ctx);
    }

    IQueryable IQueryProvider.CreateQuery(Expression expression)
    {
        if (expression is null)
            throw new ArgumentNullException(nameof(expression));

        // TODO(matt) widen acceptable types here and/or validate
        var exprTy = expression.Type;

        if (exprTy.GenericTypeArguments.Length != 1)
            throw new ArgumentException($"{nameof(expression)} type is invalid: {exprTy}");

        var elemTy = exprTy.GenericTypeArguments[0];
        var qTy = typeof(QuerySource<>).MakeGenericType(elemTy);

        return (IQueryable)Activator.CreateInstance(qTy, expression)!;
    }

    TResult IQueryProvider.Execute<TResult>(Expression expression)
    {
        var ret = ((IQueryProvider)this).Execute(expression);
        return (TResult)ret!;
    }

    object? IQueryProvider.Execute(Expression expression)
    {
        var pl = _ctx.PipelineCache.Get(_ctx, expression);
        var res = pl.ResultAsync<T>(null);
        res.Wait();
        return res.Result;
    }

    #endregion

    #region IEnumerable members

    public IEnumerator<T> GetEnumerator()
    {
        var pe = PaginateAsync().GetAsyncEnumerator();
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

    #endregion
}
