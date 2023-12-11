namespace Fauna.Serialization.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class FaunaFieldName : Attribute
{
    public string Name { get; }

    public FaunaFieldName(string name)
    {
        Name = name;
    }
}
