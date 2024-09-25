namespace Fauna.Types;

/// <summary>
/// A wrapper class that allows <see cref="Document"/> and user-defined classes
/// to be null references.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class NullableDocument<T>
{
    /// <summary>
    /// The wrapped value.
    /// </summary>
    public T? Value { get; }

    public NullableDocument(T? value)
    {
        Value = value;
    }
}

/// <summary>
/// A class representing a null document returned by Fauna.
/// </summary>
/// <typeparam name="T"></typeparam>
public class NullDocument<T> : NullableDocument<T>
{
    /// <summary>
    /// The ID of the null document.
    /// </summary>
    public string? Id { get; }

    /// <summary>
    /// The Name of the null document if it's a named document.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// The Collection.
    /// </summary>
    public Module Collection { get; }

    /// <summary>
    /// The Cause for the null document.
    /// </summary>
    public string Cause { get; }

    /// <summary>
    /// Whether the NullDocument is Named.
    /// </summary>
    public bool IsNamed { get; }

    public NullDocument(string? id, string? name, Module collection, string cause) : base(default)
    {
        if (id != null && name != null) throw new ArgumentException("Provide an id or a name, but not both.");

        Id = id;
        Name = name;
        Collection = collection;
        Cause = cause;
    }
}


/// <summary>
/// A class wrapping a non-null document returned by Fauna.
/// </summary>
/// <typeparam name="T"></typeparam>
public class NonNullDocument<T> : NullableDocument<T>
{
    public NonNullDocument(T value) : base(value)
    {
    }
}
