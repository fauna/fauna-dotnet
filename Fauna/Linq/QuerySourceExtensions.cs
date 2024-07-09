using Fauna.Linq;

namespace Fauna;

public static class QuerySourceExtensions
{
    public static Dictionary<K, V> ToDictionary<K, V>(this IQuerySource<ValueTuple<K, V>> src) where K : notnull =>
        src.ToDictionary(t => t.Item1, t => t.Item2);
    public static Task<Dictionary<K, V>> ToDictionaryAsync<K, V>(this IQuerySource<ValueTuple<K, V>> src, CancellationToken cancel = default) where K : notnull =>
        src.ToDictionaryAsync(t => t.Item1, t => t.Item2, cancel);

    public static Dictionary<K, V> ToDictionary<K, V>(this IQuerySource<ValueTuple<K, V>> src, IEqualityComparer<K>? comparer) where K : notnull =>
        src.ToDictionary(t => t.Item1, t => t.Item2, comparer);
    public static Task<Dictionary<K, V>> ToDictionaryAsync<K, V>(this IQuerySource<ValueTuple<K, V>> src, IEqualityComparer<K>? comparer, CancellationToken cancel = default) where K : notnull =>
        src.ToDictionaryAsync(t => t.Item1, t => t.Item2, comparer, cancel);

    public static int Sum(this IQuerySource<int> src) => src.Sum(x => x);
    public static Task<int> SumAsync(this IQuerySource<int> src, CancellationToken cancel = default) =>
        src.SumAsync(x => x, cancel);

    public static long Sum(this IQuerySource<long> src) => src.Sum(x => x);
    public static Task<long> SumAsync(this IQuerySource<long> src, CancellationToken cancel = default) =>
        src.SumAsync(x => x, cancel);

    public static double Sum(this IQuerySource<double> src) => src.Sum(x => x);
    public static Task<double> SumAsync(this IQuerySource<double> src, CancellationToken cancel = default) =>
        src.SumAsync(x => x, cancel);

    public static int Average(this IQuerySource<int> src) => src.Sum(x => x) / src.Count();
    public static Task<int> AverageAsync(this IQuerySource<int> src, CancellationToken cancel = default) =>
        src.AverageAsync(x => x, cancel);
    public static long Average(this IQuerySource<long> src) => (long)(src.Sum(x => x) / src.Count());
    public static Task<long> AverageAsync(this IQuerySource<long> src, CancellationToken cancel = default) =>
        src.AverageAsync(x => x, cancel);
    public static double Average(this IQuerySource<double> src) => src.Sum(x => x) / src.Count();
    public static Task<double> AverageAsync(this IQuerySource<double> src, CancellationToken cancel = default) =>
        src.AverageAsync(x => x, cancel);
}
