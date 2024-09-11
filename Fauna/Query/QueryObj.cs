using Fauna.Mapping;
using Fauna.Serialization;

namespace Fauna;

/// <summary>
/// Represents a generic value holder for FQL queries. This class allows embedding values of various types into the query, with support for primitives, POCOs, and other types.
/// </summary>
public sealed class QueryObj : Query, IQueryFragment
{
    /// <summary>
    /// Gets the value of the specified type represented in the query.
    /// </summary>
    public IDictionary<string, Query> Unwrap { get; }

    /// <summary>
    /// Initializes a new instance of the QueryObj class with the specified value.
    /// </summary>
    /// <param name="v">The value of the specified type to be represented in the query.</param>
    public QueryObj(IDictionary<string, Query> v)
    {
        Unwrap = v;
    }

    /// <summary>
    /// Serializes the query value.
    /// </summary>
    /// <param name="ctx">The serialization context.</param>
    /// <param name="writer">The writer to serialize the query value to.</param>
    public override void Serialize(MappingContext ctx, Utf8FaunaWriter writer)
    {
        var ser = Serializer.Generate(ctx, GetType());
        ser.Serialize(ctx, writer, this);
    }

    /// <summary>
    /// Determines whether the specified QueryObj is equal to the current QueryObj.
    /// </summary>
    /// <param name="o">The QueryObj to compare with the current QueryObj.</param>
    /// <returns>true if the specified QueryObj is equal to the current QueryObj; otherwise, false.</returns>
    public override bool Equals(Query? o) => IsEqual(o as QueryObj);

    /// <summary>
    /// Determines whether the specified object is equal to the current QueryObj.
    /// </summary>
    /// <param name="o">The object to compare with the current QueryObj.</param>
    /// <returns>true if the specified object is equal to the current QueryObj; otherwise, false.</returns>
    public override bool Equals(object? o)
    {
        if (ReferenceEquals(this, o))
        {
            return true;
        }

        return IsEqual(o as QueryObj);
    }

    /// <summary>
    /// The default hash function.
    /// </summary>
    /// <returns>A hash code for the current QueryObj.</returns>
    public override int GetHashCode()
    {
        var hash = 31;

        if (Unwrap is not null)
        {
            hash *= Unwrap.GetHashCode();
        }

        return hash;
    }

    /// <summary>
    /// Returns a string that represents the current QueryObj.
    /// </summary>
    /// <returns>A string that represents the current QueryObj.</returns>
    public override string ToString() => $"QueryObj({Unwrap})";

    /// <summary>
    /// Determines whether two specified instances of QueryObj are equal.
    /// </summary>
    /// <param name="left">The first QueryObj to compare.</param>
    /// <param name="right">The second QueryObj to compare.</param>
    /// <returns>true if left and right are equal; otherwise, false.</returns>
    public static bool operator ==(QueryObj left, QueryObj right)
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
    /// Determines whether two specified instances of QueryObj are not equal.
    /// </summary>
    /// <param name="left">The first QueryObj to compare.</param>
    /// <param name="right">The second QueryObj to compare.</param>
    /// <returns>true if left and right are not equal; otherwise, false.</returns>
    public static bool operator !=(QueryObj left, QueryObj right)
    {
        return !(left == right);
    }

    private bool IsEqual(QueryObj? o)
    {
        if (o is null)
        {
            return false;
        }

        if (Unwrap is null)
        {
            return (o.Unwrap is null) ? true : false;
        }

        return Unwrap.Equals(o.Unwrap);
    }
}
