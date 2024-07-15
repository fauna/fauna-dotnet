using Fauna.Mapping;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Fauna.Linq;
using QH = Fauna.Linq.IntermediateQueryHelpers;

namespace Fauna;

public abstract class DataContext : BaseClient
{
    private bool _initialized;
    [AllowNull]
    private IReadOnlyDictionary<Type, ICollection> _collections;
    [AllowNull]
    private Client _client;
    [AllowNull]
    private MappingContext _ctx;

    internal override MappingContext MappingCtx { get => _ctx; }
    internal Linq.LookupTable LookupTable { get => new(_ctx); }

    internal void Init(Client client, Dictionary<Type, ICollection> collections, MappingContext ctx)
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

    public interface ICollection : Linq.IQuerySource
    {
        public string Name { get; }
        public Type DocType { get; }
    }

    public interface ICollectionQuerySource<Doc>
    {
        public Task<Doc> CreateAsync(Doc data);
        // public QuerySuccess<Doc> Create(Doc data);

        public Task<IEnumerable<Doc>> CreateManyAsync(IEnumerable<Doc> data);
        // public IEnumerable<Doc> CreateMany(IEnumerable<Doc> data);
    }

    public abstract class Collection<Doc> : Linq.QuerySource<Doc>, ICollection, ICollectionQuerySource<Doc>
    {
        public string Name { get; }
        public Type DocType { get => typeof(Doc); }

        public Collection()
        {
            var nameAttr = GetType().GetCustomAttribute<NameAttribute>();
            Name = nameAttr?.Name ?? typeof(Doc).Name;
            SetQuery<Doc>(QH.CollectionAll(this));
        }

        // create DSL

        public Doc Create(Doc data)
        {
            return CreateAsync(data).Result;
        }

        public async Task<Doc> CreateAsync(Doc data)
        {
            var q = QH.MethodCall(QH.Expr(Name), "create", QH.Const(data));
            return (await Ctx.QueryAsync<Doc>(q)).Data;
        }

        public IEnumerable<Doc> CreateMany(IEnumerable<Doc> data)
        {
            return CreateManyAsync(data).Result;
        }

        public async Task<IEnumerable<Doc>> CreateManyAsync(IEnumerable<Doc> data)
        {
            var q = QH.MethodCall(
                QH.Array(data.Select(d => QH.Const(d))),
                "map",
                QH.Expr("d => ").Concat(QH.Expr(Name)).Concat(".create(d)")
                );

            return (await Ctx.QueryAsync<IEnumerable<Doc>>(q)).Data;
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
            private readonly ICollection _coll;
            private readonly string _name;
            private readonly DataContext _ctx;

            public IndexCall(ICollection coll, string name, DataContext ctx)
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
        public ICollection Collection { get; }
        public string Name { get; }
        public Type DocType { get; }
        public object[] Args { get; }
    }

    public class Index<Doc> : Linq.QuerySource<Doc>, Index
    {
        public ICollection Collection { get; }
        public string Name { get; }
        public Type DocType { get => typeof(Doc); }
        public object[] Args { get; }

        internal Index(ICollection coll, string name, object[] args, DataContext ctx)
        {
            Collection = coll;
            Name = name;
            Args = args;
            Ctx = ctx;
            SetQuery<Doc>(QH.CollectionIndex(this));
        }
    }

    // UDF / Function DSL

    public interface IFunction : Linq.IQuerySource
    {
        public string Name { get; }
        public Type ReturnType { get; }
        public object[] Args { get; }
    }

    protected class FunctionCall<T>
    {
        private readonly string _name;
        private readonly DataContext _ctx;

        public FunctionCall(string name, DataContext ctx)
        {
            _name = name;
            _ctx = ctx;
        }

        public Function<T> Call() => Call(Array.Empty<object>());

        public Function<T> Call(object a1) => Call(new[] { a1 });

        public Function<T> Call(object a1, object a2) => Call(new[] { a1, a2 });

        public Function<T> Call(object a1, object a2, object a3) => Call(new[] { a1, a2, a3 });

        public Function<T> Call(object[] args) => new(_name, args, _ctx);

    }

    protected FunctionCall<T> Fn<T>(string name = "", [CallerMemberName] string callerName = "")
    {
        var fnName = name == "" ? callerName : name;
        return new FunctionCall<T>(fnName, this);
    }

    public class Function<T> : QuerySource<T>, IFunction
    {
        public string Name { get; }

        public Type ReturnType => typeof(T);

        public object[] Args { get; }

        internal Function(string name, object[] args, DataContext ctx)
        {
            Name = name;
            Args = args;
            Ctx = ctx;
            SetQuery<T>(QH.Function(this));
        }
    }

    protected Col GetCollection<Col>() where Col : ICollection
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
