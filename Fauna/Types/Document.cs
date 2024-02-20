using System.Collections;

namespace Fauna.Types;

public interface IDocumentRef : INullableDocumentRef
{
    public string Id { get; set; }
    public Module Collection { get; set; }
    public DateTime? Ts { get; set; }
}

public interface INamedDocumentRef : INullableDocumentRef
{
    public string Name { get; set; }
    public Module Collection { get; set; }
}

public interface INullableDocumentRef
{
    public string? Cause { get; set; }
}

/// <summary>
/// Represents a document ref.
/// </summary>
public class BaseRef : INullableDocumentRef
{
    public BaseRef(Module collection, string? cause = null)
    {
        Cause = cause;
        Collection = collection;
    }

    /// <summary>
    /// Gets the collection to which the ref belongs.
    /// </summary>
    public Module Collection { get; set; }

    /// <summary>
    /// Gets the Cause for a null document reference.
    /// </summary>
    public string? Cause { get; set; }
}

/// <summary>
/// Represents the base structure of a document.
/// </summary>
public class BaseDocument : BaseRef, IReadOnlyDictionary<string, object?>
{
    private readonly Dictionary<string, object?> _data;

    /// <summary>
    /// Gets the timestamp of the document.
    /// </summary>
    public DateTime? Ts { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDocument"/> class with specified collection and timestamp.
    /// </summary>
    /// <param name="coll">The collection to which the document belongs.</param>
    /// <param name="ts">The timestamp of the document.</param>
    /// <param name="cause">The cause of the null document.</param>
    public BaseDocument(Module coll, DateTime? ts, string? cause = null) : base(coll, cause)
    {
        Ts = ts;
        _data = new Dictionary<string, object?>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDocument"/> class with specified collection, timestamp, and initial data.
    /// </summary>
    /// <param name="coll">The collection to which the document belongs.</param>
    /// <param name="ts">The timestamp of the document.</param>
    /// <param name="data">Initial data for the document in key-value pairs.</param>
    /// <param name="cause">The cause of the null document.</param>
    public BaseDocument(Module coll, DateTime? ts, IDictionary<string, object?> data, string? cause = null) : base(coll, cause)
    {
        Ts = ts;
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


/// <summary>
/// Represents a document.
/// </summary>
public sealed class Document : BaseDocument, IDocumentRef
{

    /// <summary>
    /// Gets the string value of the document id.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Initializes a new instance of the Document class with the specified id, coll, and ts.
    /// </summary>
    /// <param name="id">The string value of the document id.</param>
    /// <param name="coll">The module to which the document belongs.</param>
    /// <param name="ts">The timestamp of the document.</param>
    /// <param name="cause">The cause if the document is null</param>
    public Document(string id, Module coll, DateTime? ts, string? cause = null) : base(coll, ts, cause)
    {
        Id = id;
    }

    /// <summary>
    /// Initializes a new instance of the Document class with the specified id, coll, ts, and additional data stored
    /// as key/value pairs on the instance.
    /// </summary>
    /// <param name="id">The string value of the document id.</param>
    /// <param name="coll">The module to which the document belongs.</param>
    /// <param name="ts">The timestamp of the document.</param>
    /// <param name="data">Additional data on the document.</param>
    /// <param name="cause">The cause of the null document.</param>
    public Document(string id, Module coll, DateTime? ts, Dictionary<string, object?> data, string? cause = null) : base(coll, ts, data, cause)
    {
        Id = id;
    }
}

/// <summary>
/// Represents a document ref.
/// </summary>
public class DocumentRef : BaseRef
{
    public DocumentRef(string id, Module collection, string? cause = null) : base(collection, cause)
    {
        Id = id;
    }

    /// <summary>
    /// Gets the string value of the ref id.
    /// </summary>
    public string Id { get; }
}

/// <summary>
/// Represents a document that has a "name" instead of an "id". For example, a Role document is represented as a
/// NamedDocument.
/// </summary>
public sealed class NamedDocument : BaseDocument, INamedDocumentRef
{

    /// <summary>
    /// Gets the string value of the document name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Initializes a new instance of the NamedDocument class with the specified name, coll, and ts.
    /// </summary>
    /// <param name="name">The string value of the document name.</param>
    /// <param name="coll">The module to which the document belongs.</param>
    /// <param name="ts">The timestamp of the document.</param>
    /// <param name="cause">The cause of the null document.</param>
    public NamedDocument(string name, Module coll, DateTime? ts, string? cause = null) : base(coll, ts, cause)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of the NamedDocument class with the specified name, coll, ts, and additional data stored
    /// as key/value pairs on the instance.
    /// </summary>
    /// <param name="name">The string value of the document name.</param>
    /// <param name="coll">The module to which the document belongs.</param>
    /// <param name="ts">The timestamp of the document.</param>
    /// <param name="data">Additional data on the document.</param>
    /// <param name="cause">The cause of the null document.</param>
    public NamedDocument(string name, Module coll, DateTime? ts, Dictionary<string, object?> data, string? cause = null) : base(coll, ts, data, cause)
    {
        Name = name;
    }
}


/// <summary>
/// Represents a document ref that has a "name" instead of an "id". For example, a Role document reference is
/// represented as a NamedDocumentRef.
/// </summary>
public class NamedDocumentRef : BaseRef
{
    public NamedDocumentRef(string name, Module collection, string? cause = null) : base(collection, cause)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the string value of the ref name.
    /// </summary>
    public string Name { get; }
}
