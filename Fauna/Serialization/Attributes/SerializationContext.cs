using System.Reflection;

namespace Fauna.Serialization.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class SerializationContext : Attribute
{
    
    public string? Name { get; set; }
    public FaunaType? Type { get; set; }
    
    public PropertyInfo? Info { get; set; }

    
    public SerializationContext()
    {
    }

    public SerializationContext(string name)
    {
        Name = name;
    }
    
    public SerializationContext(FaunaType type)
    {
        Type = type;
    }
    
    public SerializationContext(string name, FaunaType type)
    {
        Name = name;
        Type = type;
    }
}
