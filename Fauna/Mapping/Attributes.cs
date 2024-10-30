namespace Fauna.Mapping;


/// <summary>
/// Attribute used to indicate that a class represents a Fauna document or struct.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
[Obsolete("Object attribute is no longer used and will not influence serialization.")]
public class ObjectAttribute : Attribute
{
}

/// <summary>
/// Attribute used to indicate that a field should be ignored during serialization and deserialization.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class IgnoreAttribute : Attribute
{
    /// <summary>
    /// Initializes an instance of an <see cref="IgnoreAttribute"/>.
    /// </summary>
    public IgnoreAttribute() { }

}

/// <summary>
/// An abstract type for attributing user-defined document classes.
/// </summary>
public abstract class BaseFieldAttribute : Attribute
{
    /// <summary>
    /// The name of the field.
    /// </summary>
    public readonly string? Name;
    /// <summary>
    /// The type of the field.
    /// </summary>
    public readonly FieldType Type;

    internal BaseFieldAttribute(string? name, FieldType type)
    {
        Name = name;
        Type = type;
    }
}

/// <summary>
/// Attribute used to specify fields on a Fauna document or struct.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FieldAttribute : BaseFieldAttribute
{
    /// <summary>
    /// Initializes a <see cref="FieldAttribute"/> of type Field with no name override. The name is inferred.
    /// </summary>
    public FieldAttribute() : base(null, FieldType.Field) { }

    /// <summary>
    /// Initializes a <see cref="FieldAttribute"/> of type Field with a name.
    /// </summary>
    /// <param name="name">The name of the field as stored in Fauna.</param>
    public FieldAttribute(string name) : base(name, FieldType.Field) { }
}

/// <summary>
/// Attribute used to specify the id field on a Fauna document. The associated field will be ignored during
/// serialization unless isClientGenerated is set to true.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class IdAttribute : BaseFieldAttribute
{
    private const string FieldName = "id";

    /// <summary>
    /// Initializes a <see cref="IdAttribute"/>, indicating Fauna will generate the ID.
    /// </summary>
    public IdAttribute() : base(FieldName, FieldType.ServerGeneratedId) { }


    /// <summary>
    /// Initializes a <see cref="IdAttribute"/>.
    /// </summary>
    /// <param name="isClientGenerated">Whether the ID should be considered client-generated or not.</param>
    public IdAttribute(bool isClientGenerated)
        : base(FieldName, isClientGenerated ? FieldType.ClientGeneratedId : FieldType.ServerGeneratedId) { }
}

/// <summary>
/// Attribute used to specify the coll (Collection) field on a Fauna document. The associated field will be ignored
/// during serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CollectionAttribute : BaseFieldAttribute
{
    private const string FieldName = "coll";

    /// <summary>
    /// Initializes a <see cref="CollectionAttribute"/>. The Fauna field name will always be `coll` for this attribute.
    /// </summary>
    public CollectionAttribute() : base(FieldName, FieldType.Coll) { }

}

/// <summary>
/// Attribute used to specify the ts field on a Fauna document. The associated field will be ignored during
/// serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class TsAttribute : BaseFieldAttribute
{
    private const string FieldName = "ts";

    /// <summary>
    /// Initializes a <see cref="TsAttribute"/>. The Fauna field name will always be `ts` for this attribute.
    /// </summary>
    public TsAttribute() : base(FieldName, FieldType.Ts) { }
}
