using System.Collections;

namespace Fauna.Linq;

/// <summary>
/// Represents a Fauna query with a LINQ-style API.
/// </summary>
/// <typeparam name="T">The type returned by the query when evaluated.</typeparam>
public partial class QuerySource<T>
{
    /// <summary>
    /// Evaluates the query synchronously into a <see cref="List{T}"/>.
    /// </summary>
    /// <returns>The query result as a <see cref="List{T}"/>.</returns>
    public List<T> ToList() => ToEnumerable().ToList();

    /// <summary>
    /// Evaluates the query asynchronously into a <see cref="List{T}"/>.
    /// </summary>
    /// <returns>The query result as an awaitable <see cref="Task"/> of <see cref="List{T}"/>.</returns>
    public async Task<List<T>> ToListAsync(CancellationToken cancel = default)
    {
        var ret = new List<T>();
        await foreach (var e in ToAsyncEnumerable(cancel)) ret.Add(e);
        return ret;
    }

    /// <summary>
    /// Evaluates the query synchronously into a <see cref="T:T[]"/>.
    /// </summary>
    /// <returns>The query result as <see cref="T:T[]"/>.</returns>
    public T[] ToArray() => ToEnumerable().ToArray();

    /// <summary>
    /// Evaluates the query asynchronously into a <see cref="T:T[]"/>.
    /// </summary>
    /// <returns>The query result as an awaitable <see cref="Task"/> of <see cref="T:T[]"/>.</returns>
    public async Task<T[]> ToArrayAsync(CancellationToken cancel = default) =>
        (await ToListAsync(cancel)).ToArray();

    /// <summary>
    /// Evaluates the query synchronously into a <see cref="HashSet{T}"/>.
    /// </summary>
    /// <returns>The query result as a <see cref="HashSet{T}"/>.</returns>
    public HashSet<T> ToHashSet() => ToHashSet(null);

    /// <summary>
    /// Evaluates the query asynchronously into a <see cref="HashSet{T}"/>.
    /// </summary>
    /// <returns>The query result as an awaitable <see cref="Task"/> of <see cref="HashSet{T}"/>.</returns>
    public Task<HashSet<T>> ToHashSetAsync(CancellationToken cancel = default) =>
        ToHashSetAsync(null, cancel);

    /// <summary>
    /// Evaluates the query synchronously into a <see cref="HashSet{T}"/> using a comparer.
    /// </summary>
    /// <param name="comparer">The comparer to use.</param>
    /// <returns>The query result as a <see cref="HashSet{T}"/>.</returns>
    public HashSet<T> ToHashSet(IEqualityComparer<T>? comparer) => ToEnumerable().ToHashSet(comparer);

    /// <summary>
    /// Evaluates the query asynchronously into a <see cref="HashSet{T}"/>.
    /// </summary>
    /// <param name="comparer">The comparer to use.</param>
    /// <param name="cancel">A cancellation token.</param>
    /// <returns>The query result as an awaitable <see cref="Task"/> of <see cref="HashSet{T}"/>.</returns>
    public async Task<HashSet<T>> ToHashSetAsync(IEqualityComparer<T>? comparer, CancellationToken cancel = default)
    {
        var ret = new HashSet<T>(comparer);
        await foreach (var e in ToAsyncEnumerable(cancel)) ret.Add(e);
        return ret;
    }

    /// <summary>
    /// Evaluates the query synchronously into a <see cref="Dictionary{K,V}"/>.
    /// </summary>
    /// <param name="getKey">A function used to obtain a key.</param>
    /// <param name="getValue">A function used to obtain a value.</param>
    /// <typeparam name="K">The key type of the dictionary.</typeparam>
    /// <typeparam name="V">The value type of the dictionary.</typeparam>
    /// <returns>The query result as a <see cref="Dictionary{K,V}"/></returns>
    public Dictionary<K, V> ToDictionary<K, V>(Func<T, K> getKey, Func<T, V> getValue) where K : notnull =>
        ToDictionary(getKey, getValue, null);

    /// <summary>
    /// Evaluates the query asynchronously into a <see cref="Dictionary{K,V}"/>.
    /// </summary>
    /// <param name="getKey">A function used to obtain a key.</param>
    /// <param name="getValue">A function used to obtain a value.</param>
    /// <param name="cancel">A cancellation token.</param>
    /// <typeparam name="K">The key type of the dictionary.</typeparam>
    /// <typeparam name="V">The value type of the dictionary.</typeparam>
    /// <returns>The query result as an awaitable <see cref="Task"/> of <see cref="Dictionary{K,V}"/>.</returns>
    public Task<Dictionary<K, V>> ToDictionaryAsync<K, V>(Func<T, K> getKey, Func<T, V> getValue, CancellationToken cancel = default) where K : notnull =>
        ToDictionaryAsync(getKey, getValue, null, cancel);

    /// <summary>
    /// Evaluates the query synchronously into a <see cref="Dictionary{K,V}"/>.
    /// </summary>
    /// <param name="getKey">A function used to obtain a key.</param>
    /// <param name="getValue">A function used to obtain a value.</param>
    /// <param name="comparer">A comparer used to compare keys.</param>
    /// <typeparam name="K">The key type of the dictionary.</typeparam>
    /// <typeparam name="V">The value type of the dictionary.</typeparam>
    /// <returns>The query result as a <see cref="Dictionary{K,V}"/></returns>
    public Dictionary<K, V> ToDictionary<K, V>(Func<T, K> getKey, Func<T, V> getValue, IEqualityComparer<K>? comparer) where K : notnull =>
        ToEnumerable().ToDictionary(getKey, getValue, comparer);

    /// <summary>
    /// Evaluates the query asynchronously into a <see cref="Dictionary{K,V}"/>.
    /// </summary>
    /// <param name="getKey">A function used to obtain a key.</param>
    /// <param name="getValue">A function used to obtain a value.</param>
    /// <param name="comparer">A comparer used to compare keys.</param>
    /// <param name="cancel">A cancellation token.</param>
    /// <typeparam name="K">The key type of the dictionary.</typeparam>
    /// <typeparam name="V">The value type of the dictionary.</typeparam>
    /// <returns>The query result as an awaitable <see cref="Task"/> of <see cref="Dictionary{K,V}"/>.</returns>
    public async Task<Dictionary<K, V>> ToDictionaryAsync<K, V>(Func<T, K> getKey, Func<T, V> getValue, IEqualityComparer<K>? comparer, CancellationToken cancel = default) where K : notnull
    {
        var ret = new Dictionary<K, V>(comparer);
        await foreach (var e in ToAsyncEnumerable(cancel)) ret[getKey(e)] = getValue(e);
        return ret;
    }

    /// <summary>
    /// A struct to treat a <see cref="QuerySource{T}"/> as enumerable.
    /// </summary>
    /// <param name="Source"></param>
    public record struct QuerySourceEnumerable(QuerySource<T> Source) : IEnumerable<T>
    {
        /// <summary>
        /// Gets an enumerator for the wrapped <see cref="QuerySource{T}"/>.
        /// </summary>
        /// <returns>An enumerator.</returns>
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
