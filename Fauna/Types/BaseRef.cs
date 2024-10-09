using Fauna.Linq;

namespace Fauna.Types;

public abstract class BaseRef<T>
{

    /// <summary>
    /// Gets the materialized document represented by the Ref. Is null unless IsLoaded is true
    /// and Exists is true.
    /// </summary>
    protected readonly T? Doc;

    /// <summary>
    /// Gets the cause when exists is false. Is null unless IsLoaded is true and Exists is false.
    /// </summary>
    public string? Cause { get; }

    /// <summary>
    /// Gets the collection to which the ref belongs.
    /// </summary>
    public Module Collection { get; }

    /// <summary>
    /// Gets a boolean indicating whether the doc exists. Is null unless IsLoaded is true.
    /// </summary>
    public bool? Exists { get; }

    /// <summary>
    /// Gets a boolean indicating whether the document represented by the ref has been loaded.
    /// </summary>
    public bool IsLoaded { get; } = false;

    public BaseRef(DataContext.ICollection col)
    {
        Collection = new Module(col.Name);
    }


    public BaseRef(DataContext.ICollection col, T doc)
    {
        Collection = new Module(col.Name);
        Doc = doc;
        IsLoaded = true;
        Exists = true;
    }

    public BaseRef(DataContext.ICollection col, string cause)
    {
        Collection = new Module(col.Name);
        Exists = false;
        Cause = cause;
        IsLoaded = true;
    }

    public BaseRef(Module coll)
    {
        Collection = coll;
    }

    public BaseRef(Module coll, T doc)
    {
        Collection = coll;
        Exists = true;
        Doc = doc;
        IsLoaded = true;
    }

    public BaseRef(Module coll, string cause)
    {
        Collection = coll;
        Exists = false;
        Cause = cause;
        IsLoaded = true;
    }

    public abstract T Get();
}

internal class UnloadedRefException : Exception
{
}
