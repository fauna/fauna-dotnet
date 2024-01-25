namespace Fauna.Mapping.Attributes;

/// <summary>
/// Enumerates the different types of data that can be stored in Fauna.
/// </summary>
public enum FaunaType
{
    Int,
    Long,
    Double,
    String,
    Date,
    Time,
    Boolean,
}

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
    internal readonly FaunaType? Type;

    public FieldAttribute() { }

    public FieldAttribute(string name)
    {
        Name = name;
    }

    public FieldAttribute(FaunaType type)
    {
        Type = type;
    }

    public FieldAttribute(string name, FaunaType type)
    {
        Name = name;
        Type = type;
    }
}
