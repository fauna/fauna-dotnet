namespace Fauna;

public static class PaginationExtensions
{
    public static async IAsyncEnumerable<T> FlattenAsync<T>(this IAsyncEnumerable<Page> pages)
    {
        await foreach (var page in pages)
        {
            foreach (var item in page.GetData<T>())
            {
                yield return item;
            }
        }
    }
}

