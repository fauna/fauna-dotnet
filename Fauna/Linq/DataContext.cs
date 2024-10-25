using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Fauna.Core;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Linq;

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
        Serialization.ISerializer<T> serializer,
        MappingContext ctx,
        QueryOptions? queryOptions,
        CancellationToken cancel)
    {
        CheckInitialization();
        return _client.QueryAsyncInternal(query, serializer, ctx, queryOptions, cancel);
    }

    internal override IAsyncEnumerator<Event<T>> SubscribeStreamInternal<T>(
        EventSource eventSource,
        MappingContext ctx,
        CancellationToken cancel = default)
    {
        CheckInitialization();
        return _client.SubscribeStreamInternal<T>(eventSource, ctx, cancel);
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
        public object[] Args { get; }
    }

    protected class FunctionCall<T> where T : notnull
    {

        public string Name { get; }
        private readonly DataContext _ctx;

        public FunctionCall(string name, DataContext ctx)
        {
            Name = name;
            _ctx = ctx;
        }

        public T Call() => Call(Array.Empty<object>());

        public T Call(object a1) => Call(new[] { a1 });

        public T Call(object a1, object a2) => Call(new[] { a1, a2 });

        public T Call(object a1, object a2, object a3) => Call(new[] { a1, a2, a3 });

        public T Call(object[] args) => CallAsync(args).Result;

        public async Task<T> CallAsync() => await CallAsync(Array.Empty<object>());

        public async Task<T> CallAsync(object a1) => await CallAsync(new[] { a1 });

        public async Task<T> CallAsync(object a1, object a2) => await CallAsync(new[] { a1, a2 });

        public async Task<T> CallAsync(object a1, object a2, object a3) => await CallAsync(new[] { a1, a2, a3 });

        public async Task<T> CallAsync(object[] args)
        {
            var q = Linq.IntermediateQueryHelpers.Function(Name, args);
            return (await _ctx.QueryAsync<T>(q)).Data;
        }

    }

    protected FunctionCall<T> Fn<T>(string name = "", [CallerMemberName] string callerName = "") where T : notnull
    {
        var fnName = name == "" ? callerName : name;
        return new FunctionCall<T>(fnName, this);
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
