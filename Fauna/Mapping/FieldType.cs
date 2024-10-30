namespace Fauna.Mapping;

/// <summary>
/// An enum for the field type, used with concrete implementations of <see cref="BaseFieldAttribute"/>.
/// </summary>
public enum FieldType
{
    /// <summary>
    /// Indicates the document ID is client-generated.
    /// </summary>
    ClientGeneratedId,

    /// <summary>
    /// Indicates the document ID is Fauna-generated.
    /// </summary>
    ServerGeneratedId,

    /// <summary>
    /// Indicates the field is the coll (collection) field of the document.
    /// </summary>
    Coll,

    /// <summary>
    /// Indicates the field is the ts field of the document.
    /// </summary>
    Ts,

    /// <summary>
    /// Indicates the field contains user-defined data for the document.
    /// </summary>
    Field
}
