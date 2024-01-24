using Fauna.Mapping;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Fauna;

public abstract class DataContext : BaseClient
{
    private bool _initialized = false;
    [AllowNull]
    private IReadOnlyDictionary<Type, Collection> _collections;
    [AllowNull]
    private Client _client;
    private readonly MappingContext _ctx = new();

    protected override MappingContext MappingCtx { get => _ctx; }

    internal void Init(Client client, Dictionary<Type, Collection> collections)
    {
        _client = client;
        _collections = collections.ToImmutableDictionary();
        _initialized = true;
    }

    internal override Task<QuerySuccess<T>> QueryAsyncInternal<T>(
        Query query,
        Serialization.IDeserializer<T> deserializer,
        MappingContext ctx,
        QueryOptions? queryOptions)
    {
        CheckInitialization();
        return _client.QueryAsyncInternal(query, deserializer, ctx, queryOptions);
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
        CheckInitialization();
        return (Col)_collections[typeof(Col)];
    }

    private void CheckInitialization()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException(
                "Uninitialized context. DataContext sub-classes must be instantiated using a client's .DataContext() method.");
        }

    }
}
