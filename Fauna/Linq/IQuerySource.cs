using Fauna.Types;

namespace Fauna.Linq;

public interface IQuerySource { }

public interface IQuerySource<T> : IQuerySource
{
    public IEnumerable<T> ToEnumerable();
    public IAsyncEnumerable<T> ToAsyncEnumerable();
    public IAsyncEnumerable<Page<T>> PaginateAsync(QueryOptions? queryOptions = null);
}
