using System.Collections;

namespace Fauna.Types;

/// <summary>
/// Represents the base structure of a document.
/// </summary>
public class BaseDocument : IReadOnlyDictionary<string, object?>
{
    private readonly Dictionary<string, object?> _data;

    /// <summary>
    /// Gets the timestamp of the document.
    /// </summary>
    public DateTime Ts { get; }

    /// <summary>
    /// Gets the collection to which the document belongs.
    /// </summary>
    public Module Collection { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDocument"/> class with specified collection and timestamp.
    /// </summary>
    /// <param name="coll">The collection to which the document belongs.</param>
    /// <param name="ts">The timestamp of the document.</param>
    public BaseDocument(Module coll, DateTime ts)
    {
        Ts = ts;
        Collection = coll;
        _data = new Dictionary<string, object?>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDocument"/> class with specified collection, timestamp, and initial data.
    /// </summary>
    /// <param name="coll">The collection to which the document belongs.</param>
    /// <param name="ts">The timestamp of the document.</param>
    /// <param name="data">Initial data for the document in key-value pairs.</param>
    public BaseDocument(Module coll, DateTime ts, IDictionary<string, object?> data)
    {
        Ts = ts;
        Collection = coll;
        _data = new Dictionary<string, object?>(data);
    }

    /// <summary>Returns an enumerator that iterates through the data of the document.</summary>
    /// <returns>An enumerator for the data of the document.</returns>
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        return _data.GetEnumerator();
    }

    /// <summary>Returns an enumerator that iterates through the data of the document.</summary>
    /// <returns>An enumerator for the data of the document.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Gets the count of key-value pairs contained in the document.
    /// </summary>
    /// <value>The number of key-value pairs.</value>
    public int Count => _data.Count;


    /// <summary>Determines whether the Document contains the specified key.</summary>
    /// <param name="key">The key to locate in the Document.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="key" /> is <see langword="null" />.</exception>
    /// <returns>
    /// <see langword="true" /> if the Document contains an element with the specified key; otherwise, <see langword="false" />.</returns>
    public bool ContainsKey(string key)
    {
        return _data.ContainsKey(key);
    }

    /// <summary>Gets the value associated with the specified key.</summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="key" /> is <see langword="null" />.</exception>
    /// <returns>
    /// <see langword="true" /> if the Document contains an element with the specified key; otherwise, <see langword="false" />.</returns>
    public bool TryGetValue(string key, out object? value)
    {
        return _data.TryGetValue(key, out value);
    }

    /// <summary>
    /// Gets the value associated with the specified key in the document.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <value>The value associated with the specified key.</value>
    public object? this[string key] => _data[key];

    /// <summary>Gets a collection containing the keys of the data in the document.</summary>
    /// <returns>A collection containing the keys of the data in the document.</returns>
    public IEnumerable<string> Keys => _data.Keys;

    /// <summary>Gets a collection containing the values, excluding properties, of the Document.</summary>
    /// <returns>A collection containing the values, excluding properties, of the Document.</returns>
    public IEnumerable<object?> Values => _data.Values;
}
