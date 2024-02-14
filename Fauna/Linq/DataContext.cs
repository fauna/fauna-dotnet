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

    internal override MappingContext MappingCtx { get => _ctx; }
    internal Linq.LookupTable LookupTable { get => new Linq.LookupTable(_ctx); }

    internal void Init(Client client, Dictionary<Type, Collection> collections, MappingContext ctx)
    {
        _client = client;
        _collections = collections.ToImmutableDictionary();
        _ctx = ctx;

        foreach (var col in collections.Values)
        {
            ((Linq.QuerySource)col).SetContext(this);
        }

        _initialized = true;
    }

    // IClient impl

    internal override Task<QuerySuccess<T>> QueryAsyncInternal<T>(
        Query query,
        Serialization.IDeserializer<T> deserializer,
        MappingContext ctx,
        QueryOptions? queryOptions,
        CancellationToken cancel)
    {
        CheckInitialization();
        return _client.QueryAsyncInternal(query, deserializer, ctx, queryOptions, cancel);
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

    public interface Collection : Linq.IQuerySource
    {
        public string Name { get; }
        public Type DocType { get; }
    }

    public abstract class Collection<Doc> : Linq.QuerySource<Doc>, Collection
    {
        public string Name { get; }
        public Type DocType { get => typeof(Doc); }

        public Collection()
        {
            var nameAttr = this.GetType().GetCustomAttribute<NameAttribute>();
            Name = nameAttr?.Name ?? typeof(Doc).Name;
            SetQuery<Doc>(Linq.IntermediateExpr.CollectionAll(this));
        }

        // index call DSL

        protected IndexCall Index(string? name = null, [CallerMemberName] string? auto = null)
        {
            if (name is null && auto is not null)
            {
                name = FieldName.Canonical(auto);
            }

            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"{nameof(name)} cannot be null or empty.");

            return new IndexCall(this, name, Ctx);
        }

        protected class IndexCall
        {
            private readonly Collection _coll;
            private readonly string _name;
            private readonly DataContext _ctx;

            public IndexCall(Collection coll, string name, DataContext ctx)
            {
                _coll = coll;
                _name = name;
                _ctx = ctx;
            }

            public Index<Doc> Call() => Call(new object[] { });

            public Index<Doc> Call(object a1) => Call(new object[] { a1 });

            public Index<Doc> Call(object a1, object a2) => Call(new object[] { a1, a2 });

            public Index<Doc> Call(object a1, object a2, object a3) => Call(new object[] { a1, a2, a3 });

            public Index<Doc> Call(object[] args) => new Index<Doc>(_coll, _name, args, _ctx);

        }
    }

    public interface Index : Linq.IQuerySource
    {
        public Collection Collection { get; }
        public string Name { get; }
        public Type DocType { get; }
        public object[] Args { get; }
    }

    public class Index<Doc> : Linq.QuerySource<Doc>, Index
    {
        public Collection Collection { get; }
        public string Name { get; }
        public Type DocType { get => typeof(Doc); }
        public object[] Args { get; }

        internal Index(Collection coll, string name, object[] args, DataContext ctx)
        {
            Collection = coll;
            Name = name;
            Args = args;
            Ctx = ctx;
            SetQuery<Doc>(Linq.IntermediateExpr.CollectionIndex(this));
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
