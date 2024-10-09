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
    public IgnoreAttribute() { }

}

public abstract class BaseFieldAttribute : Attribute
{
    public readonly string? Name;
    public FieldType Type;

    protected BaseFieldAttribute(string? name, FieldType type)
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
    public FieldAttribute() : base(null, FieldType.Field) { }
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

    public IdAttribute() : base(FieldName, FieldType.ServerGeneratedId) { }

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

    public TsAttribute() : base(FieldName, FieldType.Ts) { }
}
