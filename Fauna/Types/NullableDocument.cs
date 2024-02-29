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
    public string Id { get; }

    /// <summary>
    /// The Collection.
    /// </summary>
    public Module Collection { get; }

    /// <summary>
    /// The Cause for the null document.
    /// </summary>
    public string Cause { get; }

    public NullDocument(string id, Module collection, string cause) : base(default)
    {
        Id = id;
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