namespace Fauna.Serialization;

/// <summary>
/// An enum representing possible <a href="https://docs.fauna.com/fauna/current/reference/fql/types/">Fauna types</a>.
/// </summary>
public enum FaunaType
{
    /// <summary>
    /// A Fauna integer.
    /// </summary>
    Int,

    /// <summary>
    /// A Fauna long.
    /// </summary>
    Long,

    /// <summary>
    /// A Fauna double.
    /// </summary>
    Double,

    /// <summary>
    /// A Fauna string.
    /// </summary>
    String,

    /// <summary>
    /// A Fauna date.
    /// </summary>
    Date,


    /// <summary>
    /// A Fauna time.
    /// </summary>
    Time,

    /// <summary>
    /// A Fauna boolean.
    /// </summary>
    Boolean,

    /// <summary>
    /// A Fauna object. This is different from a <see cref="Document"/>.
    /// </summary>
    Object,

    /// <summary>
    /// A Fauna document reference. This includes named documents.
    /// </summary>
    Ref,

    /// <summary>
    /// A Fauna document.
    /// </summary>
    Document,

    /// <summary>
    /// A Fauna array.
    /// </summary>
    Array,

    /// <summary>
    /// A Fauna byte array, stored as a base-64 encoded string.
    /// </summary>
    Bytes,

    /// <summary>
    /// A null value.
    /// </summary>
    Null,

    /// <summary>
    /// A Fauna event source.
    /// </summary>
    Stream,

    /// <summary>
    /// A Fauna module.
    /// </summary>
    Module,

    /// <summary>
    /// A Fauna set.
    /// </summary>
    Set
}
