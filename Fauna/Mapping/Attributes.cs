namespace Fauna.Mapping;


/// <summary>
/// Attribute used to indicate that a class represents a Fauna document or struct.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ObjectAttribute : Attribute
{
}

/// <summary>
/// Attribute used to specify properties of a field in a Fauna object.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FieldAttribute : Attribute
{
    internal readonly string? Name;

    public FieldAttribute() { }

    public FieldAttribute(string name)
    {
        Name = name;
    }
}
