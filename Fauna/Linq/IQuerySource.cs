using System.Linq.Expressions;
using Fauna.Core;
using Fauna.Types;

namespace Fauna.Linq;

/// <summary>
/// An interface for common static IQuerySource methods that are non-generic.
/// </summary>
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

/// <summary>
/// An interface defining the LINQ API for Fauna queries.
/// </summary>
/// <typeparam name="T">The type returned by the query.</typeparam>
public interface IQuerySource<T> : IQuerySource
{
    // Core execution

    /// <summary>
    /// Executes a paginating query asynchronously and returns an enumerable of pages.
    /// </summary>
    /// <param name="queryOptions">An instance of <see cref="QueryOptions"/>.</param>
    /// <param name="cancel">A cancellation token.</param>
    /// <returns>An IAsyncEnumerable of type <see cref="Page{T}"/>.</returns>
    public IAsyncEnumerable<Page<T>> PaginateAsync(QueryOptions? queryOptions = null, CancellationToken cancel = default);
    /// <summary>
    /// Executes a query asynchronously.
    /// </summary>
    /// <param name="cancel">A cancellation token.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/>.</returns>
    public IAsyncEnumerable<T> ToAsyncEnumerable(CancellationToken cancel = default);
    /// <summary>
    /// Executes a query.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/>.</returns>
    public IEnumerable<T> ToEnumerable();

    // Composition methods

    /// <summary>
    /// Obtains a distinct set of results. This is evaluated server-side.
    /// </summary>
    /// <returns>This <see cref="IQuerySource{T}"/> instance.</returns>
    public IQuerySource<T> Distinct();
    /// <summary>
    /// Applies default ordering to the query. This is evaluated server-side.
    /// </summary>
    /// <returns>This <see cref="IQuerySource{T}"/> instance.</returns>
    public IQuerySource<T> Order();
    /// <summary>
    /// Orders according to the selector. This is evaluated server-side.
    /// </summary>
    /// <returns>This <see cref="IQuerySource{T}"/> instance.</returns>
    public IQuerySource<T> OrderBy<K>(Expression<Func<T, K>> keySelector);
    /// <summary>
    /// Orders by descending. This is evaluated server-side.
    /// </summary>
    /// <returns>This <see cref="IQuerySource{T}"/> instance.</returns>
    public IQuerySource<T> OrderDescending();
    /// <summary>
    /// Orders by descending according to the selector. This is evaluated server-side.
    /// </summary>
    /// <returns>This <see cref="IQuerySource{T}"/> instance.</returns>
    public IQuerySource<T> OrderByDescending<K>(Expression<Func<T, K>> keySelector);
    /// <summary>
    /// Reverses the order of the results. This is evaluated server-side.
    /// </summary>
    /// <returns>This <see cref="IQuerySource{T}"/> instance.</returns>
    public IQuerySource<T> Reverse();
    /// <summary>
    /// Applies a projection to the query. This is evaluated server-side.
    /// </summary>
    /// <returns>This <see cref="IQuerySource{T}"/> instance.</returns>
    public IQuerySource<R> Select<R>(Expression<Func<T, R>> selector);
    // public IQuerySource<R> SelectMany<R>(Expression<Func<T, IQuerySource<R>>> selector);
    /// <summary>
    /// Skips the first N elements of the results. This is evaluated server-side.
    /// </summary>
    /// <returns>This <see cref="IQuerySource{T}"/> instance.</returns>
    public IQuerySource<T> Skip(int count);
    // public IQuerySource<R> SelectMany<R>(Expression<Func<T, IQuerySource<R>>> selector);
    /// <summary>
    /// Takes the first N elements of the results. This is evaluated server-side.
    /// </summary>
    /// <returns>This <see cref="IQuerySource{T}"/> instance.</returns>
    public IQuerySource<T> Take(int count);
    // public IQuerySource<R> SelectMany<R>(Expression<Func<T, IQuerySource<R>>> selector);
    /// <summary>
    /// Applies the predicate to the query. This is evaluated server-side.
    /// </summary>
    /// <returns>This <see cref="IQuerySource{T}"/> instance.</returns>
    public IQuerySource<T> Where(Expression<Func<T, bool>> predicate);

    // Terminal result methods

    // public R Aggregate<A, R>(A seed, Expression<Func<A, T, A>> accum, Func<A, R> selector);
    // public Task<R> AggregateAsync<A, R>(A seed, Expression<Func<A, T, A>> accum, Func<A, R> selector);

    // // not IQueryable
    // public R Fold<R>(R seed, Expression<Func<R, T, R>> accum);
    // public Task<R> FoldAsync<R>(R seed, Expression<Func<R, T, R>> accum);

    // public IQuerySource<R> SelectMany<R>(Expression<Func<T, IQuerySource<R>>> selector);

    /// <summary>
    /// Applies each predicate and executes the query. This is evaluated server-side.
    /// </summary>
    /// <returns>True if every predicate evaluates to true. Otherwise, false.</returns>
    public bool All(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Applies each predicate and executes the query asynchronously. This is evaluated server-side.
    /// </summary>
    /// <returns>True if every predicate evaluate to true, otherwise false.</returns>
    public Task<bool> AllAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default);

    /// <summary>
    /// Determines if the result is not empty. This is evaluated server-side.
    /// </summary>
    /// <returns>True if the result contains any elements, otherwise false.</returns>
    public bool Any();
    /// <summary>
    /// Determines if the result is not empty asynchronously. This is evaluated server-side.
    /// </summary>
    /// <returns>True if the result contains any elements, otherwise false.</returns>
    public Task<bool> AnyAsync(CancellationToken cancel = default);

    /// <summary>
    /// Applies each predicate and executes the query. This is evaluated server-side.
    /// </summary>
    /// <returns>True if any predicate evaluates to true, otherwise false.</returns>
    public bool Any(Expression<Func<T, bool>> predicate);


    /// <summary>
    /// Applies each predicate and executes the query asynchronously. This is evaluated server-side.
    /// </summary>
    /// <returns>True if any predicate evaluates to true, otherwise false.</returns>
    public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default);

    /// <summary>
    /// Applies a count of the elements and executes the query. This is evaluated server-side.
    /// </summary>
    /// <returns>The number of elements in the result.</returns>
    public int Count();
    /// <summary>
    /// Applies a count of the elements and executes the query asynchronously. This is evaluated server-side.
    /// </summary>
    /// <returns>The number of elements in the result.</returns>
    public Task<int> CountAsync(CancellationToken cancel = default);

    /// <summary>
    /// Applies the predicate, applies a count, and executes the query. This is evaluated server-side.
    /// </summary>
    /// <returns>The number of elements in the result.</returns>
    public int Count(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// Applies the predicate, applies a count, and executes the query asynchronously. This is evaluated server-side.
    /// </summary>
    /// <returns>The number of elements in the result.</returns>
    public Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default);

    /// <summary>
    /// Executes the query and obtains the first element in the result. This is evaluated server-side.
    /// </summary>
    /// <returns>The first element in the result.</returns>
    public T First();
    /// <summary>
    /// Executes the query asynchronously and obtains the first element in the result. This is evaluated server-side.
    /// </summary>
    /// <returns>The first element in the result.</returns>
    public Task<T> FirstAsync(CancellationToken cancel = default);

    /// <summary>
    /// Applies the predicate, executes the query, and obtains the first element in the result. This is evaluated server-side.
    /// </summary>
    /// <returns>The first element in the result.</returns>
    public T First(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// Applies the predicate, executes the query asynchronously, and obtains the first element in the result.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The first element in the result.</returns>
    public Task<T> FirstAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default);

    /// <summary>
    /// Executes the query and obtains the first element in the result
    /// or the default value, if there is no result. This is evaluated server-side.
    /// </summary>
    /// <returns>The first element in the result or the default value, if there is no result.</returns>
    public T? FirstOrDefault();
    /// <summary>
    /// Executes the query asynchronously and obtains the first element in the result
    /// or the default value, if there is no result. This is evaluated server-side.
    /// </summary>
    /// <returns>The first element in the result.</returns>
    public Task<T?> FirstOrDefaultAsync(CancellationToken cancel = default);

    /// <summary>
    /// Applies the predicate, executes the query, and obtains the first element in the result or the
    /// default value, if there is no result. This is evaluated server-side.
    /// </summary>
    /// <returns>The first element in the result.</returns>
    public T? FirstOrDefault(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// Applies the predicate, executes the query asynchronously, and obtains the first element in the result or the
    /// default value, if there is no result. This is evaluated server-side.
    /// </summary>
    /// <returns>The first element in the result.</returns>
    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default);


    /// <summary>
    /// Executes the query and obtains the last element in the result. This is evaluated server-side.
    /// </summary>
    /// <returns>The last element in the result.</returns>
    public T Last();
    /// <summary>
    /// Executes the query asynchronously and obtains the last element in the result. This is evaluated server-side.
    /// </summary>
    /// <returns>The last element in the result.</returns>
    public Task<T> LastAsync(CancellationToken cancel = default);

    /// <summary>
    /// Applies the predicate, executes the query, and obtains the last element in the result.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The last element in the result.</returns>
    public T Last(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// Applies the predicate, executes the query asynchronously, and obtains the last element in the result.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The last element in the result.</returns>
    public Task<T> LastAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default);

    /// <summary>
    /// Executes the query and obtains the last element in the result or the default value, if there is no result.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The last element in the result or the default value, if there is no result.</returns>
    public T? LastOrDefault();
    /// <summary>
    /// Executes the query asynchronously and obtains the last element in the result or the default value,
    /// if there is no result. This is evaluated server-side.
    /// </summary>
    /// <returns>The last element in the result or the default value, if there is no result.</returns>
    public Task<T?> LastOrDefaultAsync(CancellationToken cancel = default);

    /// <summary>
    /// Applies the predicate, executes the query and obtains the last element in the result
    /// or the default value, if there is no result. This is evaluated server-side.
    /// </summary>
    /// <returns>The last element in the result or the default value, if there is no result.</returns>
    public T? LastOrDefault(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// Applies the predicate, executes the query asynchronously and obtains the last element in the result
    /// or the default value, if there is no result. This is evaluated server-side.
    /// </summary>
    /// <returns>The last element in the result or the default value, if there is no result.</returns>
    public Task<T?> LastOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default);

    /// <summary>
    /// Applies a count of the elements and executes the query. This is evaluated server-side.
    /// </summary>
    /// <returns>The number of elements in the result.</returns>
    public long LongCount();
    /// <summary>
    /// Applies a count of the elements and executes the query asynchronously. This is evaluated server-side.
    /// </summary>
    /// <returns>The number of elements in the result.</returns>
    public Task<long> LongCountAsync(CancellationToken cancel = default);

    /// <summary>
    /// Applies the predicate, applies a count, and executes the query. This is evaluated server-side.
    /// </summary>
    /// <returns>The number of elements in the result.</returns>
    public long LongCount(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// Applies the predicate, applies a count, and executes the query asynchronously. This is evaluated server-side.
    /// </summary>
    /// <returns>The number of elements in the result.</returns>
    public Task<long> LongCountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default);


    /// <summary>
    /// Executes a query and returns the maximum value in the result set. This is evaluated server-side.
    /// <p/>
    /// (a, b) => if (a >= b) a else b
    /// </summary>
    /// <returns>The maximum <typeparamref name="T"/>.</returns>
    public T Max();
    /// <summary>
    /// Executes a query asynchronously and returns the maximum value in the result set. This is evaluated server-side.
    /// <p/>
    /// (a, b) => if (a >= b) a else b
    /// </summary>
    /// <returns>The maximum <typeparamref name="T"/>.</returns>
    public Task<T> MaxAsync(CancellationToken cancel = default);

    /// <summary>
    /// Applies a selector, executes a query, and returns the maximum value in the result set. This is evaluated server-side.
    /// <p/>
    /// (a, b) => if (a >= b) a else b
    /// </summary>
    /// <returns>The maximum <typeparamref name="T"/>.</returns>
    public R Max<R>(Expression<Func<T, R>> selector);
    /// <summary>
    /// Applies a selector, executes a query asynchronously, and returns the maximum value in the result set.
    /// This is evaluated server-side.
    /// <p/>
    /// (a, b) =&gt; if (a &gt;= b) a else b
    /// </summary>
    /// <returns>The maximum <typeparamref name="T"/>.</returns>
    public Task<R> MaxAsync<R>(Expression<Func<T, R>> selector, CancellationToken cancel = default);

    // public T MaxBy<K>(Expression<Func<T, K>> selector);
    // public Task<K> MaxByAsync<K>(Expression<Func<T, K>> selector, CancellationToken cancel = default);

    /// <summary>
    /// Executes a query and returns the minimum value in the result set. This is evaluated server-side.
    /// <p/>
    /// (a, b) =&gt; if (a &lt;= b) a else b
    /// </summary>
    /// <returns>The minimum <typeparamref name="T"/>.</returns>
    public T Min();
    /// <summary>
    /// Executes a query asynchronously and returns the minimum value in the result set. This is evaluated server-side.
    /// <p/>
    /// (a, b) =&gt; if (a &lt;= b) a else b
    /// </summary>
    /// <returns>The minimum <typeparamref name="T"/>.</returns>
    public Task<T> MinAsync(CancellationToken cancel = default);

    // public T MinBy<K>(Expression<Func<T, K>> selector);
    // public Task<K> MinByAsync<K>(Expression<Func<T, K>> selector, CancellationToken cancel = default);

    /// <summary>
    /// Applies the selector, executes the query, and returns the minimum value in the result set.
    /// This is evaluated server-side.
    /// <p/>
    /// (a, b) =&gt; if (a &lt;= b) a else b
    /// </summary>
    /// <returns>The minimum <typeparamref name="T"/>.</returns>
    public R Min<R>(Expression<Func<T, R>> selector);
    /// <summary>
    /// Applies the selector, executes the query asynchronously, and returns the minimum value in the result set.
    /// This is evaluated server-side.
    /// <p/>
    /// (a, b) =&gt; if (a &lt;= b) a else b
    /// </summary>
    /// <returns>The minimum <typeparamref name="T"/>.</returns>
    public Task<R> MinAsync<R>(Expression<Func<T, R>> selector, CancellationToken cancel = default);

    /// <summary>
    /// Executes the query. If the result is a single element, returns it. Otherwise, throws an exception.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The query result.</returns>
    public T Single();
    /// <summary>
    /// Executes the query asynchronously. If the result is a single element, returns it. Otherwise, throws an exception.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The query result.</returns>
    public Task<T> SingleAsync(CancellationToken cancel = default);

    /// <summary>
    /// Applies the predicate and executes the query. If the result is a single element, returns it. Otherwise, throws an exception.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The query result.</returns>
    public T Single(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// Applies the predicate and executes the query asynchronously. If the result is a single element, returns it. Otherwise, throws an exception.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The query result.</returns>
    public Task<T> SingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default);

    /// <summary>
    /// Executes the query. If the result is a single element, returns it. Otherwise, returns the default.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The query result or default.</returns>
    public T SingleOrDefault();
    /// <summary>
    /// Executes the query asynchronously. If the result is a single element, returns it. Otherwise, returns the default.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The query result or default.</returns>
    public Task<T> SingleOrDefaultAsync(CancellationToken cancel = default);

    /// <summary>
    /// Applies the predicate and executes the query. If the result is a single element, returns it. Otherwise, returns the default.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The query result or default.</returns>
    public T SingleOrDefault(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// Applies the predicate and executes the query asynchronously. If the result is a single element, returns it. Otherwise, returns the default.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The query result or default.</returns>
    public Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancel = default);

    /// <summary>
    /// Calculates the sum of values returned by a provided selector.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The sum of selected values.</returns>
    public int Sum(Expression<Func<T, int>> selector);
    /// <summary>
    /// Asynchronously calculates the sum of values returned by a provided selector.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The sum of selected values.</returns>
    public Task<int> SumAsync(Expression<Func<T, int>> selector, CancellationToken cancel = default);

    /// <summary>
    /// Calculates the sum of values returned by a provided selector.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The sum of selected values.</returns>
    public long Sum(Expression<Func<T, long>> selector);
    /// <summary>
    /// Asynchronously calculates the sum of values returned by a provided selector.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The sum of selected values.</returns>
    public Task<long> SumAsync(Expression<Func<T, long>> selector, CancellationToken cancel = default);

    // public float Sum(Expression<Func<T, float>> selector);
    // public Task<float> SumAsync(Expression<Func<T, float>> selector, CancellationToken cancel = default);

    /// <summary>
    /// Calculates the sum of values returned by a provided selector.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The sum of selected values.</returns>
    public double Sum(Expression<Func<T, double>> selector);
    /// <summary>
    /// Asynchronously calculates the sum of values returned by a provided selector.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The sum of selected values.</returns>
    public Task<double> SumAsync(Expression<Func<T, double>> selector, CancellationToken cancel = default);

    /// <summary>
    /// Calculates the mean average of values returned by a provided selector.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The mean average of selected values.</returns>
    public double Average(Expression<Func<T, double>> selector);
    /// <summary>
    /// Asynchronously calculates the mean average of values returned by a provided selector.
    /// This is evaluated server-side.
    /// </summary>
    /// <returns>The mean average of selected values.</returns>
    public Task<double> AverageAsync(Expression<Func<T, double>> selector, CancellationToken cancel = default);

    // Collection result methods

    /// <summary>
    /// Executes the query and converts the results to a <see cref="List{T}"/>.
    /// </summary>
    /// <returns>A <see cref="List{T}"/>.</returns>
    public List<T> ToList();
    /// <summary>
    /// Executes the query asynchronously and converts the results to a <see cref="List{T}"/>.
    /// </summary>
    /// <returns>A <see cref="List{T}"/>.</returns>
    public Task<List<T>> ToListAsync(CancellationToken cancel = default);

    /// <summary>
    /// Executes the query and converts the results to a <see cref="T:T[]"/>.
    /// </summary>
    /// <returns>A <see cref="T:T[]"/>.</returns>
    public T[] ToArray();
    /// <summary>
    /// Executes the query asynchronously and converts the results to a <see cref="T:T[]"/>.
    /// </summary>
    /// <returns>A <see cref="T:T[]"/>.</returns>
    public Task<T[]> ToArrayAsync(CancellationToken cancel = default);

    /// <summary>
    /// Executes the query and converts the results to a <see cref="HashSet{T}"/>.
    /// </summary>
    /// <returns>A <see cref="HashSet{T}"/>.</returns>
    public HashSet<T> ToHashSet();
    /// <summary>
    /// Executes the query asynchronously and converts the results to a <see cref="HashSet{T}"/>.
    /// </summary>
    /// <returns>A <see cref="HashSet{T}"/>.</returns>
    public Task<HashSet<T>> ToHashSetAsync(CancellationToken cancel = default);


    /// <summary>
    /// Executes the query and converts the results to a <see cref="HashSet{T}"/>.
    /// </summary>
    /// <param name="comparer">A comparer to use.</param>
    /// <returns>A <see cref="HashSet{T}"/>.</returns>
    public HashSet<T> ToHashSet(IEqualityComparer<T>? comparer);

    /// <summary>
    /// Executes the query asynchronously and converts the results to a <see cref="HashSet{T}"/>.
    /// </summary>
    /// <param name="comparer">A comparer to use.</param>
    /// <param name="cancel">A cancellation token.</param>
    /// <returns>A <see cref="HashSet{T}"/>.</returns>
    public Task<HashSet<T>> ToHashSetAsync(IEqualityComparer<T>? comparer, CancellationToken cancel = default);

    /// <summary>
    /// Executes the query synchronously and converts the results to a <see cref="Dictionary{K,V}"/>.
    /// </summary>
    /// <param name="getKey">A function used to obtain a key.</param>
    /// <param name="getValue">A function used to obtain a value.</param>
    /// <typeparam name="K">The key type of the dictionary.</typeparam>
    /// <typeparam name="V">The value type of the dictionary.</typeparam>
    /// <returns>The query result as a <see cref="Dictionary{K,V}"/>.</returns>
    public Dictionary<K, V> ToDictionary<K, V>(Func<T, K> getKey, Func<T, V> getValue) where K : notnull;
    /// <summary>
    /// Executes the query asynchronously and converts the results to a <see cref="Dictionary{K,V}"/>.
    /// </summary>
    /// <param name="getKey">A function used to obtain a key.</param>
    /// <param name="getValue">A function used to obtain a value.</param>
    /// <param name="cancel">A cancellation token.</param>
    /// <typeparam name="K">The key type of the dictionary.</typeparam>
    /// <typeparam name="V">The value type of the dictionary.</typeparam>
    /// <returns>The query result as an awaitable <see cref="Task"/> of <see cref="Dictionary{K,V}"/>.</returns>
    public Task<Dictionary<K, V>> ToDictionaryAsync<K, V>(Func<T, K> getKey, Func<T, V> getValue, CancellationToken cancel = default) where K : notnull;

    /// <summary>
    /// Executes the query synchronously and converts the results to a <see cref="Dictionary{K,V}"/>.
    /// </summary>
    /// <param name="getKey">A function used to obtain a key.</param>
    /// <param name="getValue">A function used to obtain a value.</param>
    /// <param name="comparer">A comparer used to compare keys.</param>
    /// <typeparam name="K">The key type of the dictionary.</typeparam>
    /// <typeparam name="V">The value type of the dictionary.</typeparam>
    /// <returns>The query result as a <see cref="Dictionary{K,V}"/>.</returns>
    public Dictionary<K, V> ToDictionary<K, V>(Func<T, K> getKey, Func<T, V> getValue, IEqualityComparer<K>? comparer) where K : notnull;
    /// <summary>
    /// Executes the query asynchronously and converts the results to a <see cref="Dictionary{K,V}"/>.
    /// </summary>
    /// <param name="getKey">A function used to obtain a key.</param>
    /// <param name="getValue">A function used to obtain a value.</param>
    /// <param name="comparer">A comparer used to compare keys.</param>
    /// <param name="cancel">A cancellation token.</param>
    /// <typeparam name="K">The key type of the dictionary.</typeparam>
    /// <typeparam name="V">The value type of the dictionary.</typeparam>
    /// <returns>The query result as an awaitable <see cref="Task"/> of <see cref="Dictionary{K,V}"/>.</returns>
    public Task<Dictionary<K, V>> ToDictionaryAsync<K, V>(Func<T, K> getKey, Func<T, V> getValue, IEqualityComparer<K>? comparer, CancellationToken cancel = default) where K : notnull;
}
