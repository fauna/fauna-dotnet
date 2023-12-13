using System.Reflection;

namespace Fauna.Serialization.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class FieldAttribute : Attribute
{
    public PropertyInfo? Info { get; set; }
    public string Name { get; set; }
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
