namespace Fauna;

/// <summary>
/// Provides extension methods for pagination.
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Flattens pages into a stream of items.
    /// </summary>
    /// <typeparam name="T">Data type.</typeparam>
    /// <param name="pages">Pages to flatten.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that enumerates each item across all pages.</returns>
    public static async IAsyncEnumerable<T> FlattenAsync<T>(this IAsyncEnumerable<Page<T>> pages)
    {
        await foreach (var page in pages)
        {
            foreach (var item in page.Data)
            {
                yield return item;
            }
        }
    }
}

