using System.Collections;

namespace Fauna.Types;

public sealed class Document : Ref, IDictionary<string, object?>
{
    private Dictionary<string, object?> _data = new();

    public DateTime Ts { get; set; }

    public ICollection<string> Keys => _data.Keys;
    public ICollection<object?> Values => _data.Values;

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        return _data.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_data).GetEnumerator();
    }

    public void Add(KeyValuePair<string, object?> item)
    {
        _data.Add(item.Key, item.Value);
    }

    public void Clear()
    {
        _data.Clear();
    }

    public bool Contains(KeyValuePair<string, object?> item)
    {
        return _data.Contains(item);
    }

    public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(KeyValuePair<string, object?> item)
    {
        throw new NotImplementedException();
    }

    public int Count => _data.Count;

    public bool IsReadOnly => true;

    public void Add(string key, object? value)
    {
        _data.Add(key, value);
    }

    public bool ContainsKey(string key)
    {
        return _data.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        return _data.Remove(key);
    }

    public bool TryGetValue(string key, out object? value)
    {
        throw new NotImplementedException();
    }

    public object? this[string key]
    {
        get => _data[key];
        set => _data[key] = value;
    }

}
