using System.Collections.ObjectModel;
using Fauna.Serialization;

namespace Fauna;

/// <summary>
/// Represents an FQL query expression. This class encapsulates a list of IQueryFragment instances, allowing for complex query constructions.
/// </summary>
public sealed class QueryExpr : Query, IQueryFragment
{
    /// <summary>
    /// Initializes a new instance of the QueryExpr class with a collection of query fragments.
    /// </summary>
    /// <param name="fragments">The collection of IQueryFragment instances.</param>
    public QueryExpr(IList<IQueryFragment> fragments)
    {
        Unwrap = new ReadOnlyCollection<IQueryFragment>(fragments);
    }

    /// <summary>
    /// Initializes a new instance of the QueryExpr class with one or more query fragments.
    /// </summary>
    /// <param name="fragments">The array of IQueryFragment instances.</param>
    public QueryExpr(params IQueryFragment[] fragments)
        : this(fragments.ToList())
    {
    }

    /// <summary>
    /// Gets the readonly collection of query fragments.
    /// </summary>
    public ReadOnlyCollection<IQueryFragment> Unwrap { get; }

    public ReadOnlyCollection<IQueryFragment> Fragments => Unwrap;

    public override void Serialize(Utf8FaunaWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteFieldName("fql");
        writer.WriteStartArray();
        foreach (var t in Unwrap)
        {
            t.Serialize(writer);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
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
