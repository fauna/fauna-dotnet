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

    internal abstract Task<TResult> ExecuteAsync<TResult>(Expression expression);
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
        var res = ExecuteAsync<TResult>(expression);
        res.Wait();
        return res.Result;
    }

    internal override Task<TResult> ExecuteAsync<TResult>(Expression expression)
    {
        var pl = Ctx.PipelineCache.Get(Ctx, expression);
        return pl.Result<TResult>(queryOptions: null);
    }

    public IAsyncEnumerable<Page<T>> PaginateAsync(QueryOptions? queryOptions = null)
    {
        var pl = Ctx.PipelineCache.Get(Ctx, Expr);
        return pl.PagedResult<T>(queryOptions);
    }

    public IAsyncEnumerable<T> ToAsyncEnumerable() => PaginateAsync().FlattenAsync();

    public IEnumerable<T> ToEnumerable() => new QuerySourceEnumerable(this);

    public List<T> ToList() => ToEnumerable().ToList();
    public async Task<List<T>> ToListAsync()
    {
        var ret = new List<T>();
        await foreach (var e in ToAsyncEnumerable()) ret.Add(e);
        return ret;
    }

    public T[] ToArray() => ToEnumerable().ToArray();
    public async Task<T[]> ToArrayAsync() => (await ToListAsync()).ToArray();

    public HashSet<T> ToHashSet() => ToHashSet(null);
    public Task<HashSet<T>> ToHashSetAsync() => ToHashSetAsync(null);

    public HashSet<T> ToHashSet(IEqualityComparer<T>? comparer) => ToEnumerable().ToHashSet(comparer);
    public async Task<HashSet<T>> ToHashSetAsync(IEqualityComparer<T>? comparer)
    {
        var ret = new HashSet<T>(comparer);
        await foreach (var e in ToAsyncEnumerable()) ret.Add(e);
        return ret;
    }

    public Dictionary<K, V> ToDictionary<K, V>(Func<T, K> getKey, Func<T, V> getValue) where K : notnull =>
        ToDictionary(getKey, getValue, null);
    public Task<Dictionary<K, V>> ToDictionaryAsync<K, V>(Func<T, K> getKey, Func<T, V> getValue) where K : notnull =>
        ToDictionaryAsync(getKey, getValue, null);

    public Dictionary<K, V> ToDictionary<K, V>(Func<T, K> getKey, Func<T, V> getValue, IEqualityComparer<K>? comparer) where K : notnull =>
        ToEnumerable().ToDictionary(getKey, getValue, comparer);
    public async Task<Dictionary<K, V>> ToDictionaryAsync<K, V>(Func<T, K> getKey, Func<T, V> getValue, IEqualityComparer<K>? comparer) where K : notnull
    {
        var ret = new Dictionary<K, V>(comparer);
        await foreach (var e in ToAsyncEnumerable()) ret[getKey(e)] = getValue(e);
        return ret;
    }

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
