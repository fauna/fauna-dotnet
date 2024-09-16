using Fauna.Mapping;
using Fauna.Serialization;

namespace Fauna;

/// <summary>
/// Represents a dictionary of FQL queries.
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
        return ReferenceEquals(this, o) || IsEqual(o as QueryObj);
    }

    /// <summary>
    /// The default hash function.
    /// </summary>
    /// <returns>A hash code for the current QueryObj.</returns>
    public override int GetHashCode()
    {
        int hash = 31;

        hash *= Unwrap.GetHashCode();

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
        return ReferenceEquals(left, right) || left.Equals(right);
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
        return o is not null && Unwrap.Equals(o.Unwrap);
    }
}
