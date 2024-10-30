using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using Fauna.Core;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Linq;

/// <summary>
/// An abstract class representing a DataContext. This is a special type of Fauna client that can be used to execute LINQ-style queries.
///
/// Users should implement this for each database they'd like to query with LINQ.
/// </summary>
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

    /// <summary>
    /// An attribute representing a collection name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NameAttribute : Attribute
    {
        internal readonly string Name;

        /// <summary>
        /// Initializes a <see cref="NameAttribute"/>.
        /// </summary>
        /// <param name="name">The collection name.</param>
        public NameAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// An interface for a Fauna collection within a <see cref="DataContext"/>.
    /// </summary>
    public interface ICollection : Linq.IQuerySource
    {
        /// <summary>
        /// The collection name.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The .NET type associated with documents in the collection.
        /// </summary>
        public Type DocType { get; }
    }

    /// <summary>
    /// An abstract collection. This should be implemented for each collection in the database.
    /// </summary>
    /// <typeparam name="Doc">The .NET type associated with documents in the collection.</typeparam>
    public abstract class Collection<Doc> : Linq.QuerySource<Doc>, ICollection
    {
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Type DocType { get => typeof(Doc); }

        /// <summary>
        /// Initializes a new collection with a name set to the <see cref="NameAttribute"/>, or the name of
        /// .NET type associated with its documents.
        /// </summary>
        public Collection()
        {
            var nameAttr = this.GetType().GetCustomAttribute<NameAttribute>();
            Name = nameAttr?.Name ?? typeof(Doc).Name;
            SetQuery<Doc>(Linq.IntermediateQueryHelpers.CollectionAll(this));
        }

        // index call DSL

        /// <summary>
        /// Initializes an index associated with the collection. The name of the index can be assigned, or if
        /// declared inside a concrete <see cref="DataContext"/>, a canonical name will be assigned when a name
        /// is not provieded.
        /// </summary>
        /// <param name="name">The name of the index.</param>
        /// <param name="auto">Used to generate a canonical name when name is null.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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

        /// <summary>
        /// A class representing an index call.
        /// </summary>
        protected class IndexCall
        {
            private readonly ICollection _coll;
            private readonly string _name;
            private readonly DataContext _ctx;

            /// <summary>
            /// Initializes an index call.
            /// </summary>
            /// <param name="coll">The collection the index belongs to.</param>
            /// <param name="name">The name of the index.</param>
            /// <param name="ctx">The <see cref="DataContext"/></param>
            public IndexCall(ICollection coll, string name, DataContext ctx)
            {
                _coll = coll;
                _name = name;
                _ctx = ctx;
            }

            /// <summary>
            /// Invokes an index.
            /// </summary>
            /// <returns>An <see cref="Index{Doc}"/> query source.</returns>
            public Index<Doc> Call() => Call(new object[] { });

            /// <summary>
            /// Invokes an index.
            /// </summary>
            /// <param name="a1">An index argument.</param>
            /// <returns>An <see cref="Index{Doc}"/> query source.</returns>
            public Index<Doc> Call(object a1) => Call(new object[] { a1 });

            /// <summary>
            /// Invokes an index.
            /// </summary>
            /// <param name="a1">An index argument.</param>
            /// <param name="a2">An index argument.</param>
            /// <returns>An <see cref="Index{Doc}"/> query source.</returns>
            public Index<Doc> Call(object a1, object a2) => Call(new object[] { a1, a2 });

            /// <summary>
            /// Invokes an index.
            /// </summary>
            /// <param name="a1">An index argument.</param>
            /// <param name="a2">An index argument.</param>
            /// <param name="a3">An index argument.</param>
            /// <returns>An <see cref="Index{Doc}"/> query source.</returns>
            public Index<Doc> Call(object a1, object a2, object a3) => Call(new object[] { a1, a2, a3 });

            /// <summary>
            /// Invokes an index.
            /// </summary>
            /// <param name="args">An array of index arguments.</param>
            /// <returns>An <see cref="Index{Doc}"/> query source.</returns>
            public Index<Doc> Call(object[] args) => new Index<Doc>(_coll, _name, args, _ctx);

        }
    }

    /// <summary>
    /// An interface representing an index query source.
    /// </summary>
    public interface IIndex : Linq.IQuerySource
    {
        /// <summary>
        /// The collection the index belongs to.
        /// </summary>
        public ICollection Collection { get; }
        /// <summary>
        /// The name of the index.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The return type of the index.
        /// </summary>
        public Type DocType { get; }
        /// <summary>
        /// An index argument array.
        /// </summary>
        public object[] Args { get; }
    }

    /// <summary>
    /// A class representing an index query source.
    /// </summary>
    /// <typeparam name="Doc"></typeparam>
    public class Index<Doc> : Linq.QuerySource<Doc>, IIndex
    {
        /// <inheritdoc />
        public ICollection Collection { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Type DocType { get => typeof(Doc); }

        /// <inheritdoc />
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

    /// <summary>
    /// An interface representing a function.
    /// </summary>
    public interface IFunction : Linq.IQuerySource
    {
        /// <summary>
        /// The name of the function.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// An array of arguments for the function.
        /// </summary>
        public object[] Args { get; }
    }

    /// <summary>
    /// A class representing a function call.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    protected class FunctionCall<T> where T : notnull
    {

        /// <summary>
        /// The name of the function.
        /// </summary>
        public string Name { get; }
        private readonly DataContext _ctx;

        /// <summary>
        /// Initializes a function call.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="ctx">The <see cref="DataContext"/> that the function belongs to.</param>
        public FunctionCall(string name, DataContext ctx)
        {
            Name = name;
            _ctx = ctx;
        }

        /// <summary>
        /// Calls the function.
        /// </summary>
        /// <returns>The result of the function call.</returns>
        public T Call() => Call(Array.Empty<object>());

        /// <summary>
        /// Calls the function.
        /// </summary>
        /// <param name="a1">A function call argument.</param>
        /// <returns>The result of the function call.</returns>
        public T Call(object a1) => Call(new[] { a1 });

        /// <summary>
        /// Calls the function.
        /// </summary>
        /// <param name="a1">A function call argument.</param>
        /// <param name="a2">A function call argument.</param>
        /// <returns>The result of the function call.</returns>
        public T Call(object a1, object a2) => Call(new[] { a1, a2 });

        /// <summary>
        /// Calls the function.
        /// </summary>
        /// <param name="a1">A function call argument.</param>
        /// <param name="a2">A function call argument.</param>
        /// <param name="a3">A function call argument.</param>
        /// <returns>The result of the function call.</returns>
        public T Call(object a1, object a2, object a3) => Call(new[] { a1, a2, a3 });

        /// <summary>
        /// Calls the function.
        /// </summary>
        /// <param name="args">An array of function call arguments.</param>
        /// <returns>The result of the function call.</returns>
        public T Call(object[] args) => CallAsync(args).Result;

        /// <summary>
        /// Calls the function asynchronously.
        /// </summary>
        /// <returns>The result of the function call.</returns>
        public async Task<T> CallAsync() => await CallAsync(Array.Empty<object>());

        /// <summary>
        /// Calls the function asynchronously.
        /// </summary>
        /// <param name="a1">A function call argument.</param>
        /// <returns>The result of the function call.</returns>
        public async Task<T> CallAsync(object a1) => await CallAsync(new[] { a1 });

        /// <summary>
        /// Calls the function asynchronously.
        /// </summary>
        /// <param name="a1">A function call argument.</param>
        /// <param name="a2">A function call argument.</param>
        /// <returns>The result of the function call.</returns>
        public async Task<T> CallAsync(object a1, object a2) => await CallAsync(new[] { a1, a2 });

        /// <summary>
        /// Calls the function asynchronously.
        /// </summary>
        /// <param name="a1">A function call argument.</param>
        /// <param name="a2">A function call argument.</param>
        /// <param name="a3">A function call argument.</param>
        /// <returns>The result of the function call.</returns>
        public async Task<T> CallAsync(object a1, object a2, object a3) => await CallAsync(new[] { a1, a2, a3 });

        /// <summary>
        /// Calls the function asynchronously.
        /// </summary>
        /// <param name="args">An array of function call arguments.</param>
        /// <returns>The result of the function call.</returns>
        public async Task<T> CallAsync(object[] args)
        {
            var q = Linq.IntermediateQueryHelpers.Function(Name, args);
            return (await _ctx.QueryAsync<T>(q)).Data;
        }

    }

    /// <summary>
    /// A helper method to declare new function calls.
    /// </summary>
    /// <param name="name">The name of the function. If null, the caller name is used.</param>
    /// <param name="callerName">The caller name.</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected FunctionCall<T> Fn<T>(string name = "", [CallerMemberName] string callerName = "") where T : notnull
    {
        var fnName = name == "" ? callerName : name;
        return new FunctionCall<T>(fnName, this);
    }

    /// <summary>
    /// Gets a collection of type <typeparamref name="Col"/>.
    /// </summary>
    /// <typeparam name="Col">The type of the collection.</typeparam>
    /// <returns>The collection</returns>
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
