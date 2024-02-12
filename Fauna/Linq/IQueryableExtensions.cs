using Fauna.Linq;
using Fauna.Types;

namespace Fauna;

public static class IQueryableExtensions
{
    public static QuerySource<T> AsQuerySource<T>(this IQueryable<T> query)
    {
        if (query is QuerySource<T> source)
        {
            return source;
        }

        throw new ArgumentException($"{nameof(query)} is not a QuerySource.");
    }

    public static IAsyncEnumerable<Page<T>> PaginateAsync<T>(this IQueryable<T> query, QueryOptions? queryOptions = null) =>
        AsQuerySource(query).PaginateAsync(queryOptions);

    public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IQueryable<T> query) =>
        AsQuerySource(query).AsAsyncEnumerable();
}
