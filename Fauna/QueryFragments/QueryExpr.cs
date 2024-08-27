using System.Collections.ObjectModel;
using Fauna.Mapping;
using Fauna.Serialization;

namespace Fauna.QueryFragments;

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
    public IReadOnlyCollection<IQueryFragment> Unwrap { get; }

    /// <summary>
    /// Gets the readonly collection of query fragments.
    /// </summary>
    public IReadOnlyCollection<IQueryFragment> Fragments => Unwrap;

    /// <summary>
    /// Serializes the query expression.
    /// </summary>
    /// <param name="ctx">The serialization context.</param>
    /// <param name="writer">The writer to serialize the query expression to.</param>
    public override void Serialize(MappingContext ctx, Utf8FaunaWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteFieldName("fql");
        writer.WriteStartArray();
        foreach (var t in Unwrap)
        {
            t.Serialize(ctx, writer);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    /// <summary>
    /// Determines whether the specified QueryExpr is equal to the current QueryExpr.
    /// </summary>
    /// <param name="o">The QueryExpr to compare with the current QueryExpr.</param>
    /// <returns>true if the specified QueryExpr is equal to the current QueryExpr; otherwise, false.</returns>
    public override bool Equals(Query? o) => IsEqual(o as QueryExpr);

    /// <summary>
    /// Determines whether the specified object is equal to the current QueryExpr.
    /// </summary>
    /// <param name="o">The object to compare with the current QueryExpr.</param>
    /// <returns>true if the specified object is equal to the current QueryExpr; otherwise, false.</returns>
    public override bool Equals(object? o)
    {
        if (ReferenceEquals(this, o))
        {
            return true;
        }

        return o is QueryExpr expr && IsEqual(expr);
    }

    /// <summary>
    /// The default hash function.
    /// </summary>
    /// <returns>A hash code for the current QueryExpr.</returns>
    public override int GetHashCode() => Fragments.GetHashCode();

    /// <summary>
    /// Returns a string that represents the current QueryExpr.
    /// </summary>
    /// <returns>A string that represents the current QueryExpr.</returns>
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

    /// <summary>
    /// Determines whether two specified instances of QueryExpr are equal.
    /// </summary>
    /// <param name="left">The first QueryExpr to compare.</param>
    /// <param name="right">The second QueryExpr to compare.</param>
    /// <returns>true if left and right are equal; otherwise, false.</returns>
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

    /// <summary>
    /// Determines whether two specified instances of QueryExpr are not equal.
    /// </summary>
    /// <param name="left">The first QueryExpr to compare.</param>
    /// <param name="right">The second QueryExpr to compare.</param>
    /// <returns>true if left and right are not equal; otherwise, false.</returns>
    public static bool operator !=(QueryExpr left, QueryExpr right)
    {
        return !(left == right);
    }
}
