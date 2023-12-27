using Fauna.Serialization;

namespace Fauna;

/// <summary>
/// Represents a generic value holder for FQL queries. This class allows embedding values of various types into the query, with support for primitives, POCOs, and other types.
/// </summary>
/// <typeparam name="T">The type of the value to be embedded in the query.</typeparam>
public sealed class QueryVal<T> : Query, IQueryFragment
{
    /// <summary>
    /// Initializes a new instance of the QueryVal class with the specified value.
    /// </summary>
    /// <param name="v">The value of the specified type to be represented in the query.</param>
    public QueryVal(T v)
    {
        Unwrap = v;
    }

    /// <summary>
    /// Gets the value of the specified type represented in the query.
    /// </summary>
    public T Unwrap { get; }

    public override void Serialize(Utf8FaunaWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteFieldName("value");
        Serializer.Serialize(writer, Unwrap);
        writer.WriteEndObject();
    }

    public override bool Equals(Query? o) => IsEqual(o as QueryVal<T>);

    public override bool Equals(object? o)
    {
        if (ReferenceEquals(this, o))
        {
            return true;
        }

        if (o is null)
        {
            return false;
        }

        if (GetType() != o.GetType())
        {
            return false;
        }

        return IsEqual(o as QueryVal<T>);
    }

    public override int GetHashCode() => Unwrap != null ? EqualityComparer<T>.Default.GetHashCode(Unwrap) : 0;

    public override string ToString() => $"QueryVal({Unwrap})";

    public static bool operator ==(QueryVal<T> left, QueryVal<T> right)
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

    public static bool operator !=(QueryVal<T> left, QueryVal<T> right)
    {
        return !(left == right);
    }

    private bool IsEqual(QueryVal<T>? o)
    {
        if (o is null)
        {
            return false;
        }

        return EqualityComparer<T>.Default.Equals(Unwrap, o.Unwrap);
    }
}
