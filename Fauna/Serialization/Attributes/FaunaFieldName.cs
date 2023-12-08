namespace Fauna.Serialization.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class FaunaFieldName : Attribute
{
    private readonly string _name;

    public FaunaFieldName(string name)
    {
        _name = name;
    }

    public string GetName() => _name;
}
