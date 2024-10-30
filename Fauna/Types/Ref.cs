using Fauna.Exceptions;
using Fauna.Linq;

namespace Fauna.Types;


/// <summary>
/// Represents a document ref.
/// </summary>
public class Ref<T> : BaseRef<T>
{
    /// <summary>
    /// Gets the string value of the ref ID.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Initializes a new instance of an unloaded <see cref="Ref{T}"/> class.
    /// </summary>
    /// <param name="id">The ID of the document.</param>
    /// <param name="col">The collection to which the document belongs.</param>
    public Ref(string id, DataContext.ICollection col) : base(col)
    {
        Id = id;
    }

    /// <summary>
    /// Initializes a new instance of a loaded <see cref="Ref{T}"/> class.
    /// </summary>
    /// <param name="id">The ID of the document.</param>
    /// <param name="col">The collection to which the document belongs.</param>
    /// <param name="doc">The instance of <typeparamref name="T" /> referenced.</param>
    public Ref(string id, DataContext.ICollection col, T doc) : base(col, doc)
    {
        Id = id;
    }

    /// <summary>
    /// Initializes a new instance of a loaded and non-existent <see cref="Ref{T}"/> class.
    /// </summary>
    /// <param name="id">The ID of the document.</param>
    /// <param name="col">The collection to which the document belongs.</param>
    /// <param name="cause">A string representing the cause for non-existence.</param>
    public Ref(string id, DataContext.ICollection col, string cause) : base(col, cause)
    {
        Id = id;
    }

    /// <summary>
    /// Initializes a new instance of an unloaded <see cref="Ref{T}"/> class.
    /// </summary>
    /// <param name="id">The ID of the document.</param>
    /// <param name="col">The collection to which the document belongs.</param>
    public Ref(string id, Module col) : base(col)
    {
        Id = id;
    }

    /// <summary>
    /// Initializes a new instance of a loaded and non-existent <see cref="Ref{T}"/> class.
    /// </summary>
    /// <param name="id">The ID of the document.</param>
    /// <param name="col">The collection to which the document belongs.</param>
    /// <param name="cause">A string representing the cause for non-existence.</param>
    public Ref(string id, Module col, string cause) : base(col, cause)
    {
        Id = id;
    }

    /// <summary>
    /// Initializes a new instance of a loaded <see cref="Ref{T}"/> class.
    /// </summary>
    /// <param name="id">The ID of the document.</param>
    /// <param name="col">The collection to which the document belongs.</param>
    /// <param name="doc">The instance of <typeparamref name="T" /> referenced.</param>
    public Ref(string id, Module col, T doc) : base(col, doc)
    {
        Id = id;
    }


    /// <inheritdoc />
    /// <exception cref="UnloadedRefException">Thrown when IsLoaded is false.</exception>
    /// <exception cref="NullDocumentException">Thrown when Exists is false.</exception>
    public override T Get()
    {
        if (!IsLoaded) throw new UnloadedRefException();
        if (Exists.HasValue && !Exists.Value) throw new NullDocumentException(Id, null, Collection, Cause ?? "");
        return Doc!;
    }
}
