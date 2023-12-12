using System.Reflection;

namespace Fauna.Serialization.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class FieldAttribute : Attribute
{
    public PropertyInfo? Info { get; set; }
    public string Name { get; set; }

    public FieldAttribute(string name)
    {
        Name = name;
    }
    
}
