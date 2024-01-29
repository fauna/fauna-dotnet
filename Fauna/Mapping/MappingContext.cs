using System.Diagnostics.CodeAnalysis;

namespace Fauna.Mapping;

public sealed class MappingContext
{
    // FIXME(matt) possibly replace with more efficient cache impl
    private readonly Dictionary<Type, MappingInfo> _cache = new();
    private readonly Dictionary<string, MappingInfo> _collections = new();

    public MappingContext() { }

    public MappingContext(IEnumerable<DataContext.Collection> collections)
    {
        foreach (var col in collections)
        {
            _collections[col.Name] = GetInfo(col.DocType);
        }
    }

    public MappingContext(Dictionary<string, Type> collections)
    {
        foreach (var (name, ty) in collections)
        {
            _collections[name] = GetInfo(ty);
        }
    }

    public bool TryGetCollection(string col, [NotNullWhen(true)] out MappingInfo? ret)
    {
        return _collections.TryGetValue(col, out ret);
    }

    public MappingInfo GetInfo(Type ty)
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
        return new MappingInfo(this, ty);
    }

    internal void Add(Type ty, MappingInfo info)
    {
        lock (this)
        {
            _cache[ty] = info;
        }
    }
}
