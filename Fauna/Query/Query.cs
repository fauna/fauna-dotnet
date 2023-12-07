namespace Fauna;

public abstract partial class Query : IEquatable<Query>
{
    public abstract bool Equals(Query? o);

    public override bool Equals(object? o) => o is Query query && Equals(query);

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static Query FQL(ref QueryStringHandler handler)
    {
        return handler.Result();
    }
}
