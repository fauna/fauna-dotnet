namespace Fauna.Mapping;

/// <summary>
/// An enum for the field type, used with concrete implementations of <see cref="BaseFieldAttribute"/>.
/// </summary>
public enum FieldType
{
    /// <summary>
    /// Indicates the document ID is client generated.
    /// </summary>
    ClientGeneratedId,

    /// <summary>
    /// Indicates the document ID is Fauna generated.
    /// </summary>
    ServerGeneratedId,

    /// <summary>
    /// Indicates the field is the Collection field on a document.
    /// </summary>
    Coll,

    /// <summary>
    /// Indicates the field is the Ts field on a document.
    /// </summary>
    Ts,

    /// <summary>
    /// Indicates the field is user-defined data on a document.
    /// </summary>
    Field
}
