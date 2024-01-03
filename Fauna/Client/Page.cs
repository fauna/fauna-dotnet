using Fauna.Serialization;

public class Page
{
    private readonly object _data;
    private readonly string? _after;
    private readonly Dictionary<Type, object?> _cache = new();

    public string? After => _after;

    public Page(object data, string? after)
    {
        _data = data;
        _after = after;
    }

    public IEnumerable<object> GetData()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<T> GetData<T>()
    {
        var typeKey = typeof(T);
        if (!_cache.TryGetValue(typeKey, out var cachedData))
        {
            var dataString = _data.ToString();
            if (!string.IsNullOrEmpty(dataString))
            {
                cachedData = Serializer.Deserialize<List<T>>(dataString);
                _cache[typeKey] = cachedData;
            }
        }
        return cachedData as IEnumerable<T> ?? Enumerable.Empty<T>();
    }
}