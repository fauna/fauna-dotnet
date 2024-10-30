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

    /// <summary>
    /// Initializes a new instance of an unloaded <see cref="NamedRef{T}"/> class.
    /// </summary>
    /// <param name="name">The name of the document.</param>
    /// <param name="col">The collection to which the document belongs.</param>
    public NamedRef(string name, DataContext.ICollection col) : base(col)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of a loaded <see cref="NamedRef{T}"/> class.
    /// </summary>
    /// <param name="name">The name of the document.</param>
    /// <param name="col">The collection to which the document belongs.</param>
    /// <param name="doc">The instance of <typeparamref name="T" /> referenced.</param>
    public NamedRef(string name, DataContext.ICollection col, T doc) : base(col, doc)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of a loaded and non-existent <see cref="NamedRef{T}"/> class.
    /// </summary>
    /// <param name="name">The name of the document.</param>
    /// <param name="col">The collection to which the document belongs.</param>
    /// <param name="cause">A string representing the cause for non-existence.</param>
    public NamedRef(string name, DataContext.ICollection col, string cause) : base(col, cause)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of an unloaded <see cref="NamedRef{T}"/> class.
    /// </summary>
    /// <param name="name">The name of the document.</param>
    /// <param name="col">The collection to which the document belongs.</param>
    public NamedRef(string name, Module col) : base(col)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of a loaded and non-existent <see cref="NamedRef{T}"/> class.
    /// </summary>
    /// <param name="name">The name of the document.</param>
    /// <param name="col">The collection to which the document belongs.</param>
    /// <param name="cause">A string representing the cause for non-existence.</param>
    public NamedRef(string name, Module col, string cause) : base(col, cause)
    {
        Name = name;
    }

    /// <summary>
    /// Initializes a new instance of a loaded <see cref="NamedRef{T}"/> class.
    /// </summary>
    /// <param name="name">The name of the document.</param>
    /// <param name="col">The collection to which the document belongs.</param>
    /// <param name="doc">The instance of <typeparamref name="T" /> referenced.</param>
    public NamedRef(string name, Module col, T doc) : base(col, doc)
    {
        Name = name;
    }

    /// <inheritdoc />
    /// <exception cref="UnloadedRefException">Thrown when IsLoaded is false.</exception>
    /// <exception cref="NullDocumentException">Thrown when Exists is false.</exception>
    public override T Get()
    {
        if (!IsLoaded) throw new UnloadedRefException();
        if (Exists.HasValue && !Exists.Value) throw new NullDocumentException(null, Name, Collection, Cause ?? "");
        return Doc!;
    }
}
