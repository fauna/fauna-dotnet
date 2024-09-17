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

/// <summary>
/// Attribute used to specify fields on a Fauna document or struct.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FieldAttribute : Attribute
{
    internal readonly string? _name;

    public FieldAttribute() { }

    public FieldAttribute(string name)
    {
        _name = name;
    }
}

/// <summary>
/// Attribute used to specify the id field on a Fauna document. The associated field will be ignored during
/// serialization unless isClientGenerated is set to true.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class IdAttribute : Attribute
{
    internal readonly bool _isClientGenerated;

    public IdAttribute() { }

    public IdAttribute(bool isClientGenerated)
    {
        _isClientGenerated = isClientGenerated;
    }
}

/// <summary>
/// Attribute used to specify the coll (Collection) field on a Fauna document. The associated field will be ignored
/// during serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class CollAttribute : Attribute
{
    public CollAttribute() { }

}

/// <summary>
/// Attribute used to specify the ts field on a Fauna document. The associated field will be ignored during
/// serialization.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class TsAttribute : Attribute
{
    public TsAttribute() { }
}
