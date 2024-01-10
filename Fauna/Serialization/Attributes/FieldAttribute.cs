using System.Reflection;

namespace Fauna.Serialization.Attributes;

/// <summary>
/// Attribute used to specify properties of a field in a Fauna object.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FieldAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the property information associated with this field.
    /// </summary>
    public PropertyInfo? Info { get; set; }

    /// <summary>
    /// Gets or sets the name of the field.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the type of the field.
    /// </summary>
    public FaunaType? Type { get; set; }


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
