using Fauna.Serialization;

public class Page
{
    public List<object> Data { get; init; }

    public string? After { get; init; }

    private readonly Dictionary<Type, List<object>> cache = new();

    public Page(List<object> rawJsonData, string? after)
    {
        Data = rawJsonData;
        After = after;
    }

    public List<T> GetData<T>()
    {
        var typeKey = typeof(T);
        if (!cache.TryGetValue(typeKey, out var cachedData))
        {
            var deserializedData = Data.Select(item => Serializer.Deserialize<T>(item.ToString())).ToList();
            cache[typeKey] = deserializedData.Cast<object>().ToList();
            return deserializedData;
        }
        return cachedData.Cast<T>().ToList();
    }
}