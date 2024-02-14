using System.Collections;

namespace Fauna.Linq;

public partial class QuerySource<T>
{
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
