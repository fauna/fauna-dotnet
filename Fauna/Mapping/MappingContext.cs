using Fauna.Serialization;

namespace Fauna.Mapping;

public sealed class MappingContext
{
    // FIXME(matt) possibly replace with more efficient cache impl
    private readonly Dictionary<Type, MappingInfo> _cache = new();

    internal readonly SerializationContext _serCtx = new();

    public MappingInfo Get(Type ty)
    {
        lock (_cache)
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
        lock (_cache)
        {
            _cache[ty] = info;
        }
    }
}
