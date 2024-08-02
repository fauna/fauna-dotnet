using Fauna.Mapping;
using Fauna.Serialization;

namespace Fauna;

/// <summary>
/// Represents a generic value holder for FQL queries. This class allows embedding values of various types into the query, with support for primitives, POCOs, and other types.
/// </summary>
public sealed class QueryVal : Query, IQueryFragment
{
    /// <summary>
    /// Gets the value of the specified type represented in the query.
    /// </summary>
    public object? Unwrap { get; }

    /// <summary>
    /// Initializes a new instance of the QueryVal class with the specified value.
    /// </summary>
    /// <param name="v">The value of the specified type to be represented in the query.</param>
    public QueryVal(object? v)
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
        writer.WriteStartObject();
        writer.WriteFieldName("value");
        var ser = Unwrap is not null ? SerializerProvider.Generate(ctx, Unwrap.GetType()) : DynamicSerializer.Singleton;
        ser.Serialize(ctx, writer, Unwrap);
        writer.WriteEndObject();
    }

    /// <summary>
    /// Determines whether the specified QueryVal is equal to the current QueryVal.
    /// </summary>
    /// <param name="o">The QueryVal to compare with the current QueryVal.</param>
    /// <returns>true if the specified QueryVal is equal to the current QueryVal; otherwise, false.</returns>
    public override bool Equals(Query? o) => IsEqual(o as QueryVal);

    /// <summary>
    /// Determines whether the specified object is equal to the current QueryVal.
    /// </summary>
    /// <param name="o">The object to compare with the current QueryVal.</param>
    /// <returns>true if the specified object is equal to the current QueryVal; otherwise, false.</returns>
    public override bool Equals(object? o)
    {
        if (ReferenceEquals(this, o))
        {
            return true;
        }

        return IsEqual(o as QueryVal);
    }

    /// <summary>
    /// The default hash function.
    /// </summary>
    /// <returns>A hash code for the current QueryVal.</returns>
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
    /// Returns a string that represents the current QueryVal.
    /// </summary>
    /// <returns>A string that represents the current QueryVal.</returns>
    public override string ToString() => $"QueryVal({Unwrap})";

    /// <summary>
    /// Determines whether two specified instances of QueryVal are equal.
    /// </summary>
    /// <param name="left">The first QueryVal to compare.</param>
    /// <param name="right">The second QueryVal to compare.</param>
    /// <returns>true if left and right are equal; otherwise, false.</returns>
    public static bool operator ==(QueryVal left, QueryVal right)
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
    /// Determines whether two specified instances of QueryVal are not equal.
    /// </summary>
    /// <param name="left">The first QueryVal to compare.</param>
    /// <param name="right">The second QueryVal to compare.</param>
    /// <returns>true if left and right are not equal; otherwise, false.</returns>
    public static bool operator !=(QueryVal left, QueryVal right)
    {
        return !(left == right);
    }

    private bool IsEqual(QueryVal? o)
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
