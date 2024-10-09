using Fauna.Exceptions;
using Fauna.Linq;

namespace Fauna.Types;


/// <summary>
/// Represents a document ref that has a "name" instead of an "id". For example, a Role document reference is
/// represented as a NamedRef.
/// </summary>
public class NamedRef<T> : BaseRef<T>
{
    /// <summary>
    /// Gets the string value of the ref name.
    /// </summary>
    public string Name { get; }

    public NamedRef(string name, DataContext.ICollection col) : base(col)
    {
        Name = name;
    }

    public NamedRef(string name, DataContext.ICollection col, T doc) : base(col, doc)
    {
        Name = name;
    }

    public NamedRef(string name, DataContext.ICollection col, string cause) : base(col, cause)
    {
        Name = name;
    }

    public NamedRef(string name, Module col) : base(col)
    {
        Name = name;
    }

    public NamedRef(string name, Module col, string cause) : base(col, cause)
    {
        Name = name;
    }

    public NamedRef(string name, Module col, T doc) : base(col, doc)
    {
        Name = name;
    }

    public override T Get()
    {
        if (!IsLoaded) throw new UnloadedRefException();
        if (Exists.HasValue && !Exists.Value) throw new NullDocumentException(null, Name, Collection, Cause ?? "");
        return Doc!;
    }
}
