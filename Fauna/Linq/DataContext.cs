using Fauna.Linq;
using Fauna.Mapping;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

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

        var provider = new QueryProvider(this);
        foreach (var col in collections.Values)
        {
            ((QuerySource)col).SetProvider(provider);
        }

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

    [AttributeUsage(AttributeTargets.Class)]
    public class NameAttribute : Attribute
    {
        internal readonly string Name;

        public NameAttribute(string name)
        {
            Name = name;
        }
    }

    public interface Collection : IQueryable
    {
        public string Name { get; }
        public Type DocType { get; }
    }

    public abstract class Collection<Doc> : QuerySource<Doc>, Collection
    {
        public string Name { get; }
        public Type DocType { get => typeof(Doc); }

        public Collection()
        {
            var nameAttr = this.GetType().GetCustomAttribute<NameAttribute>();
            Name = nameAttr?.Name ?? typeof(Doc).Name;
            _expr = Expression.Constant(this);
        }

        public Index<Doc> All() => Index().Call();

        // index call DSL

        protected IndexCall Index(string? name = null, [CallerMemberName] string? auto = null)
        {
            if (name is null && auto is not null)
            {
                name = FieldName.Canonical(auto);
            }

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"{nameof(name)} cannot be null or empty.");

            return new IndexCall(this, name, _provider);
        }

        protected class IndexCall
        {
            private readonly Collection _coll;
            private readonly string _name;
            private readonly QueryProvider _provider;

            public IndexCall(Collection coll, string name, QueryProvider provider)
            {
                _coll = coll;
                _name = name;
                _provider = provider;
            }

            public Index<Doc> Call() => Call(new object[] { });

            public Index<Doc> Call(object a1) => Call(new object[] { a1 });

            public Index<Doc> Call(object a1, object a2) => Call(new object[] { a1, a2 });

            public Index<Doc> Call(object a1, object a2, object a3) => Call(new object[] { a1, a2, a3 });

            public Index<Doc> Call(object[] args) => new Index<Doc>(_coll, _name, args, _provider);

        }
    }

    public interface Index : IQueryable
    {
        public Collection Collection { get; }
        public string Name { get; }
        public Type DocType { get; }
        public object[] Args { get; }
    }

    public class Index<Doc> : QuerySource<Doc>, Index
    {
        public Collection Collection { get; }
        public string Name { get; }
        public Type DocType { get => typeof(Doc); }
        public object[] Args { get; }

        internal Index(Collection coll, string name, object[] args, QueryProvider provider)
        {
            Collection = coll;
            Name = name;
            Args = args;
            _expr = Expression.Constant(this);
            _provider = provider;
        }
    }

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