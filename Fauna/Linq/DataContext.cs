using Fauna.Linq;
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
    [AllowNull]
    private MappingContext _ctx;

    protected override MappingContext MappingCtx { get => _ctx; }

    internal void Init(Client client, Dictionary<Type, Collection> collections, MappingContext ctx)
    {
        _client = client;
        _collections = collections.ToImmutableDictionary();
        _ctx = ctx;
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

    [AttributeUsage(AttributeTargets.Interface)]
    public class NameAttribute : Attribute
    {
        internal readonly string Name;

        public NameAttribute(string name)
        {
            Name = name;
        }
    }

    public interface Collection : IQuerySource { }

    public interface Collection<Doc> : Collection, IQuerySource<Doc>
    {
        public Index<Doc> All();
    }

    public interface Index<Doc> : IQuerySource<Doc> { }

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
