using Fauna.Mapping;

namespace Fauna;

public abstract class DatabaseContext : BaseClient
{
    private readonly MappingContext _ctx = new();
    private Client? _client;

    protected override MappingContext MappingCtx { get => _ctx; }

    internal void SetClient(Client client)
    {
        _client = client;
    }

    internal override Task<QuerySuccess<T>> QueryAsyncInternal<T>(
        Query query,
        Serialization.IDeserializer<T> deserializer,
        MappingContext ctx,
        QueryOptions? queryOptions)
    {
        if (_client is null)
        {
            throw new InvalidOperationException(
                "No associated client. DatabaseContext sub-classes must be instantiated using a client's .DatabaseContext() method.");
        }
        return _client!.QueryAsyncInternal(query, deserializer, ctx, queryOptions);
    }

    // Schema DSL

    // TODO(matt) inherit from LINQ query base
    public interface Collection { }

    // TODO(matt) inherit from LINQ query base
    public interface Collection<Doc> : Collection
    {
        public Index<Doc> All();
    }

    // TODO(matt) inherit from LINQ query base
    public interface Index<Doc> { }

    protected Col GetCollection<Col>() where Col : Collection
    {
        throw new NotImplementedException();
    }
}
