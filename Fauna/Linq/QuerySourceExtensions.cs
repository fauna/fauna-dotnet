using Fauna.Linq;

namespace Fauna;

public static class QuerySourceExtensions
{
    public static Dictionary<K, V> ToDictionary<K, V>(this IQuerySource<ValueTuple<K, V>> src) where K : notnull =>
        src.ToDictionary(t => t.Item1, t => t.Item2);
    public static Task<Dictionary<K, V>> ToDictionaryAsync<K, V>(this IQuerySource<ValueTuple<K, V>> src) where K : notnull =>
        src.ToDictionaryAsync(t => t.Item1, t => t.Item2);

    public static Dictionary<K, V> ToDictionary<K, V>(this IQuerySource<ValueTuple<K, V>> src, IEqualityComparer<K>? comparer) where K : notnull =>
        src.ToDictionary(t => t.Item1, t => t.Item2, comparer);
    public static Task<Dictionary<K, V>> ToDictionaryAsync<K, V>(this IQuerySource<ValueTuple<K, V>> src, IEqualityComparer<K>? comparer) where K : notnull =>
        src.ToDictionaryAsync(t => t.Item1, t => t.Item2, comparer);

    public static int Sum(this IQuerySource<int> src) => src.Sum(x => x);
    public static Task<int> SumAsync(this IQuerySource<int> src) => src.SumAsync(x => x);

    public static long Sum(this IQuerySource<long> src) => src.Sum(x => x);
    public static Task<long> SumAsync(this IQuerySource<long> src) => src.SumAsync(x => x);

    public static float Sum(this IQuerySource<float> src) => src.Sum(x => x);
    public static Task<float> SumAsync(this IQuerySource<float> src) => src.SumAsync(x => x);

    public static double Sum(this IQuerySource<double> src) => src.Sum(x => x);
    public static Task<double> SumAsync(this IQuerySource<double> src) => src.SumAsync(x => x);
}
