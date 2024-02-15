using Fauna.Types;
using System.Linq.Expressions;

namespace Fauna.Linq;

public interface IQuerySource
{
    // TODO(matt) use an API-specific exception in-line with what other LINQ
    // libraries do.
    internal static Exception Fail(Expression? expr) =>
        Fail($"Unsupported {expr?.NodeType} expression: {expr}");

    internal static Exception Fail(string op, string msg) =>
        Fail($"Unsupported method call `{op}`: {msg}");

    internal static Exception Fail(string msg) => new NotSupportedException(msg);
}

public interface IQuerySource<T> : IQuerySource
{
    // Core execution

    public IAsyncEnumerable<Page<T>> PaginateAsync(QueryOptions? queryOptions = null);
    public IAsyncEnumerable<T> ToAsyncEnumerable();
    public IEnumerable<T> ToEnumerable();

    // Composition methods

    public IQuerySource<T> Distinct();
    public IQuerySource<T> Order();
    public IQuerySource<T> OrderBy<K>(Expression<Func<T, K>> keySelector);
    public IQuerySource<T> OrderDescending();
    public IQuerySource<T> OrderByDescending<K>(Expression<Func<T, K>> keySelector);
    public IQuerySource<T> Reverse();
    public IQuerySource<R> Select<R>(Expression<Func<T, R>> selector);
    // public IQuerySource<R> SelectMany<R>(Expression<Func<T, IQuerySource<R>>> selector);
    public IQuerySource<T> Skip(int count);
    public IQuerySource<T> Take(int count);
    public IQuerySource<T> Where(Expression<Func<T, bool>> predicate);

    // Terminal result methods

    // public R Aggregate<A, R>(A seed, Expression<Func<A, T, A>> accum, Func<A, R> selector);
    // public Task<R> AggregateAsync<A, R>(A seed, Expression<Func<A, T, A>> accum, Func<A, R> selector);

    // // not IQueryable
    // public R Fold<R>(R seed, Expression<Func<R, T, R>> accum);
    // public Task<R> FoldAsync<R>(R seed, Expression<Func<R, T, R>> accum);

    public bool All(Expression<Func<T, bool>> predicate);
    public Task<bool> AllAsync(Expression<Func<T, bool>> predicate);

    public bool Any();
    public Task<bool> AnyAsync();

    public bool Any(Expression<Func<T, bool>> predicate);
    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

    public int Count();
    public Task<int> CountAsync();

    public int Count(Expression<Func<T, bool>> predicate);
    public Task<int> CountAsync(Expression<Func<T, bool>> predicate);

    public T First();
    public Task<T> FirstAsync();

    public T First(Expression<Func<T, bool>> predicate);
    public Task<T> FirstAsync(Expression<Func<T, bool>> predicate);

    public T? FirstOrDefault();
    public Task<T?> FirstOrDefaultAsync();

    public T? FirstOrDefault(Expression<Func<T, bool>> predicate);
    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    public T Last();
    public Task<T> LastAsync();

    public T Last(Expression<Func<T, bool>> predicate);
    public Task<T> LastAsync(Expression<Func<T, bool>> predicate);

    public T LastOrDefault();
    public Task<T> LastOrDefaultAsync();

    public T LastOrDefault(Expression<Func<T, bool>> predicate);
    public Task<T> LastOrDefaultAsync(Expression<Func<T, bool>> predicate);

    public long LongCount();
    public Task<long> LongCountAsync();

    public long LongCount(Expression<Func<T, bool>> predicate);
    public Task<long> LongCountAsync(Expression<Func<T, bool>> predicate);

    public T Max();
    public Task<T> MaxAsync();

    public R Max<R>(Expression<Func<T, R>> selector);
    public Task<R> MaxAsync<R>(Expression<Func<T, R>> selector);

    // public T MaxBy<K>(Expression<Func<T, K>> selector);
    // public Task<K> MaxByAsync<K>(Expression<Func<T, K>> selector);

    public T Min();
    public Task<T> MinAsync();

    // public T MinBy<K>(Expression<Func<T, K>> selector);
    // public Task<K> MinByAsync<K>(Expression<Func<T, K>> selector);

    public R Min<R>(Expression<Func<T, R>> selector);
    public Task<R> MinAsync<R>(Expression<Func<T, R>> selector);

    // public T Single();
    // public Task<T> SingleAsync();

    // public T Single(Expression<Func<T, bool>> predicate);
    // public Task<T> SingleAsync(Expression<Func<T, bool>> predicate);

    // public T SingleOrDefault();
    // public Task<T> SingleOrDefaultAsync();

    // public T SingleOrDefault(Expression<Func<T, bool>> predicate);
    // public Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);

    public int Sum(Expression<Func<T, int>> selector);
    public Task<int> SumAsync(Expression<Func<T, int>> selector);

    public long Sum(Expression<Func<T, long>> selector);
    public Task<long> SumAsync(Expression<Func<T, long>> selector);

    public float Sum(Expression<Func<T, float>> selector);
    public Task<float> SumAsync(Expression<Func<T, float>> selector);

    public double Sum(Expression<Func<T, double>> selector);
    public Task<double> SumAsync(Expression<Func<T, double>> selector);

    // Collection result methods

    public List<T> ToList();
    public Task<List<T>> ToListAsync();

    public T[] ToArray();
    public Task<T[]> ToArrayAsync();

    public HashSet<T> ToHashSet();
    public Task<HashSet<T>> ToHashSetAsync();

    public HashSet<T> ToHashSet(IEqualityComparer<T>? comparer);
    public Task<HashSet<T>> ToHashSetAsync(IEqualityComparer<T>? comparer);

    public Dictionary<K, V> ToDictionary<K, V>(Func<T, K> getKey, Func<T, V> getValue) where K : notnull;
    public Task<Dictionary<K, V>> ToDictionaryAsync<K, V>(Func<T, K> getKey, Func<T, V> getValue) where K : notnull;

    public Dictionary<K, V> ToDictionary<K, V>(Func<T, K> getKey, Func<T, V> getValue, IEqualityComparer<K>? comparer) where K : notnull;
    public Task<Dictionary<K, V>> ToDictionaryAsync<K, V>(Func<T, K> getKey, Func<T, V> getValue, IEqualityComparer<K>? comparer) where K : notnull;
}
