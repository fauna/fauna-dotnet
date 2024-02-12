using Fauna.Types;

namespace Fauna.Linq;

public interface IQuerySource { }

public interface IQuerySource<T> : IQuerySource
{
    public IAsyncEnumerable<Page<T>> PaginateAsync(QueryOptions? queryOptions = null);

    public IAsyncEnumerable<T> ToAsyncEnumerable();
    public IEnumerable<T> ToEnumerable();

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
