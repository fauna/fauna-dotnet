using Fauna.Mapping;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Fauna;

public abstract class DataContext : BaseClient
{
    private bool _initialized = false;
    [AllowNull]
    private IReadOnlyDictionary<Type, ICollection> _collections = null!;
    [AllowNull]
    private Client _client = null!;
    [AllowNull]
    private MappingContext _ctx = null!;

    internal override MappingContext MappingCtx { get => _ctx; }
    internal Linq.LookupTable LookupTable { get => new Linq.LookupTable(_ctx); }

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

    public abstract class Collection<Doc> : Linq.QuerySource<Doc>, ICollection
    {
        public string Name { get; }
        public Type DocType { get => typeof(Doc); }

        public Collection()
        {
            var nameAttr = this.GetType().GetCustomAttribute<NameAttribute>();
            Name = nameAttr?.Name ?? typeof(Doc).Name;
            SetQuery<Doc>(Linq.IntermediateQueryHelpers.CollectionAll(this));
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

    public interface IIndex : Linq.IQuerySource
    {
        public ICollection Collection { get; }
        public string Name { get; }
        public Type DocType { get; }
        public object[] Args { get; }
    }

    public class Index<Doc> : Linq.QuerySource<Doc>, IIndex
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
            SetQuery<Doc>(Linq.IntermediateQueryHelpers.CollectionIndex(this));
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

    public class Function<T> : Linq.QuerySource<T>, IFunction
    {
        public string Name { get; }

        public Type ReturnType => typeof(T);

        public object[] Args { get; }

        internal Function(string name, object[] args, DataContext ctx)
        {
            Name = name;
            Args = args;
            Ctx = ctx;
            SetQuery<T>(Linq.IntermediateQueryHelpers.Function(this));
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
