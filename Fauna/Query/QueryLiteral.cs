using Fauna.Serialization;

namespace Fauna;

/// <summary>
/// Represents a literal part of an FQL query. This class is used for embedding raw string values directly into the query structure.
/// </summary>
public sealed class QueryLiteral : IQueryFragment
{
    /// <summary>
    /// Initializes a new instance of the QueryLiteral class with the specified value.
    /// </summary>
    /// <param name="v">The string value to be represented as a query literal.</param>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public QueryLiteral(string v)
    {
        if (v == null)
        {
            throw new ArgumentNullException(nameof(v), "Value cannot be null.");
        }

        Unwrap = v;
    }

    /// <summary>
    /// Gets the string value of the query literal.
    /// </summary>
    public string Unwrap { get; }

    public override string ToString()
    {
        return $"QueryLiteral({Unwrap})";
    }

    public void Serialize(Utf8FaunaWriter writer)
    {
        writer.WriteStringValue(Unwrap);
    }

    public override bool Equals(object? other)
    {
        var otherQuery = other as IQueryFragment;

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
