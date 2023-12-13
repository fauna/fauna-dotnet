namespace Fauna;

public abstract class Query : IEquatable<Query>, IQueryFragment
{
    public abstract void Serialize(Stream stream);

    public abstract override int GetHashCode();

    public abstract override bool Equals(object? otherObject);

    public abstract bool Equals(Query? otherQuery);

    public static Query FQL(ref QueryStringHandler handler)
    {
        return handler.Result();
    }
}
