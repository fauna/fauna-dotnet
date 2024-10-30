using System.Diagnostics.CodeAnalysis;
using Fauna.Linq;

namespace Fauna.Mapping;

/// <summary>
/// A class representing the mapping context to be used during serialization and deserialization.
/// </summary>
public sealed class MappingContext
{
    // FIXME(matt) possibly replace with more efficient cache impl
    private readonly Dictionary<Type, MappingInfo> _cache = new();
    private readonly Dictionary<string, MappingInfo> _collections = new();
    private readonly Dictionary<Type, MappingInfo> _baseTypes = new();

    /// <summary>
    /// Initialize an empty <see cref="MappingContext"/>.
    /// </summary>
    public MappingContext() { }

    /// <summary>
    /// Initialize a <see cref="MappingContext"/> with associated collections.
    /// </summary>
    /// <param name="collections">The collections to associate.</param>
    public MappingContext(IEnumerable<DataContext.ICollection> collections)
    {
        foreach (var col in collections)
        {
            var info = GetInfo(col.DocType, col.Name);
            _collections[col.Name] = info;
            _baseTypes[col.DocType] = info;
        }
    }

    /// <summary>
    /// Initialize a <see cref="MappingContext"/> with associated collections.
    /// </summary>
    /// <param name="collections">The collections to associate.</param>
    public MappingContext(Dictionary<string, Type> collections)
    {
        foreach ((string name, Type ty) in collections)
        {
            _collections[name] = GetInfo(ty, name);
        }
    }

    /// <summary>
    /// Gets the <see cref="MappingInfo"/> for a given collection name.
    /// </summary>
    /// <param name="col">The collection name to get.</param>
    /// <param name="ret">When this method returns, contains the <see cref="MappingInfo"/> associated with the collection if found; otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if the <see cref="MappingContext"/> contains <see cref="MappingInfo"/> for the specified collection; otherwise, <c>false</c>.</returns>
    public bool TryGetCollection(string col, [NotNullWhen(true)] out MappingInfo? ret)
    {
        return _collections.TryGetValue(col, out ret);
    }

    /// <summary>
    /// Gets the <see cref="MappingInfo"/> for a given <see cref="Type"/>.
    /// </summary>
    /// <param name="ty">The type to get.</param>
    /// <param name="ret">When this method returns, contains the <see cref="MappingInfo"/> associated with the type if found; otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
    /// <returns><c>true</c> if the <see cref="MappingContext"/> contains <see cref="MappingInfo"/> for the specified type; otherwise, <c>false</c>.</returns>
    public bool TryGetBaseType(Type ty, [NotNullWhen(true)] out MappingInfo? ret)
    {
        return _baseTypes.TryGetValue(ty, out ret);
    }

    /// <summary>
    /// Gets the <see cref="MappingInfo"/> for a given <see cref="Type"/>.
    /// </summary>
    /// <param name="ty">The type to get.</param>
    /// <param name="colName">The associated collection name, if any</param>
    /// <returns></returns>
    public MappingInfo GetInfo(Type ty, string? colName = null)
    {
        lock (this)
        {
            if (_cache.TryGetValue(ty, out var ret))
            {
                return ret;
            }
        }

        // MappingInfo caches itself during construction in order to make
        // itself available early for recursive lookup.
        return new MappingInfo(this, ty, colName);
    }

    internal void Add(Type ty, MappingInfo info)
    {
        lock (this)
        {
            _cache[ty] = info;
        }
    }
}
