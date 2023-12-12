namespace Fauna;

public sealed class QueryLiteral : Query, IQueryFragment
{
    public QueryLiteral(string v)
    {
        if (v == null)
        {
            throw new ArgumentNullException(nameof(v), "Value cannot be null.");
        }

        Unwrap = v;
    }

    public string Unwrap { get; }

    public override string ToString()
    {
        return $"QueryLiteral({Unwrap})";
    }

    public override bool Equals(Query? otherQuery)
    {
        if (otherQuery is null)
        {
            return false;
        }

        if (ReferenceEquals(this, otherQuery))
        {
            return true;
        }

        if (otherQuery is QueryLiteral otherLiteral)
        {
            return Unwrap == otherLiteral.Unwrap;
        }

        return false;
    }

    public override bool Equals(object? otherObject)
    {
        return Equals(otherObject as Query);
    }

    public override int GetHashCode()
    {
        return Unwrap.GetHashCode();
    }

    public static bool operator ==(QueryLiteral left, QueryLiteral right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(QueryLiteral left, QueryLiteral right)
    {
        return !Equals(left, right);
    }
}