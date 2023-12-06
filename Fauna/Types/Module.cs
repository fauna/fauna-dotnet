namespace Fauna.Types;

public class Module: IEquatable<Module>
{
    public string Name { get; }

    public Module(string name)
    {
        Name = name;
    }

    public bool Equals(Module? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Module)obj);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}
