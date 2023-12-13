using System.Collections.ObjectModel;
using System.Text;

namespace Fauna;

public sealed class QueryExpr : Query, IQueryFragment
{
    public QueryExpr(IList<IQueryFragment> fragments)
    {
        Unwrap = new ReadOnlyCollection<IQueryFragment>(fragments);
    }

    public QueryExpr(params IQueryFragment[] fragments)
        : this(fragments.ToList())
    {
    }

    public ReadOnlyCollection<IQueryFragment> Unwrap { get; }

    public ReadOnlyCollection<IQueryFragment> Fragments => Unwrap;

    public override string Serialize()
    {
        var serializedFragments = new StringBuilder();

        serializedFragments.Append("{\"fql\":[");

        for (int i = 0; i < Unwrap.Count; i++)
        {
            var fragment = Unwrap[i].Serialize();
            serializedFragments.Append($"{fragment}");

            if (i < Unwrap.Count - 1)
            {
                serializedFragments.Append(",");
            }
        }

        serializedFragments.Append("]}");

        return serializedFragments.ToString();
    }

    public override bool Equals(Query? o) => IsEqual(o as QueryExpr);

    public override bool Equals(object? o)
    {
        if (ReferenceEquals(this, o))
        {
            return true;
        }

        return o is QueryExpr expr && IsEqual(expr);
    }

    public override int GetHashCode() => Fragments.GetHashCode();

    public override string ToString() => $"QueryExpr({string.Join(",", Fragments)})";

    private bool IsEqual(QueryExpr? o)
    {
        if (o is null)
        {
            return false;
        }

        if (Fragments == null || o.Fragments == null)
        {
            return Fragments == null && o.Fragments == null;
        }

        return Fragments.SequenceEqual(o.Fragments);
    }

    public static bool operator ==(QueryExpr left, QueryExpr right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(QueryExpr left, QueryExpr right)
    {
        return !(left == right);
    }
}
