namespace Fauna.Types;

/// <summary>
/// Represents a module, a singleton object grouping related functionalities.
/// Modules are serialized as \@mod values in tagged formats, organizing and encapsulating specific functionalities.
/// </summary>
public sealed class Module : IEquatable<Module>
{
    /// <summary>
    /// Gets the name of the module. The name is used to identify and reference the module.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the Module class with the specified name.
    /// </summary>
    /// <param name="name">The name of the module.</param>
    public Module(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Determines whether the specified Module is equal to the current Module.
    /// </summary>
    /// <param name="other">The Module to compare with the current Module.</param>
    /// <returns>true if the specified Module is equal to the current Module; otherwise, false.</returns>
    public bool Equals(Module? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Name == other.Name;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current Module.
    /// </summary>
    /// <param name="obj">The object to compare with the current Module.</param>
    /// <returns>true if the specified object is equal to the current Module; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Module)obj);
    }

    /// <summary>
    /// The default hash function.
    /// </summary>
    /// <returns>A hash code for the current Module.</returns>
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}
