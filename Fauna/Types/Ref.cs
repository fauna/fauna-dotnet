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

    public Ref(string id, DataContext.ICollection col) : base(col)
    {
        Id = id;
    }

    public Ref(string id, DataContext.ICollection col, T doc) : base(col, doc)
    {
        Id = id;
    }

    public Ref(string id, DataContext.ICollection col, string cause) : base(col, cause)
    {
        Id = id;
    }

    public Ref(string id, Module col) : base(col)
    {
        Id = id;
    }

    public Ref(string id, Module col, string cause) : base(col, cause)
    {
        Id = id;
    }

    public Ref(string id, Module col, T doc) : base(col, doc)
    {
        Id = id;
    }

    public override T Get()
    {
        if (!IsLoaded) throw new UnloadedRefException();
        if (Exists.HasValue && !Exists.Value) throw new NullDocumentException(Id, null, Collection, Cause ?? "");
        return Doc!;
    }
}
