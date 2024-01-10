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

    /// <summary>
    /// Returns a string that represents the current QueryLiteral.
    /// </summary>
    /// <returns>A string that represents the current QueryLiteral.</returns>
    public override string ToString()
    {
        return $"QueryLiteral({Unwrap})";
    }

    /// <summary>
    /// Serializes the query literal.
    /// </summary>
    /// <param name="ctx">The serialization context.</param>
    /// <param name="writer">The writer to serialize the query literal to.</param>
    public void Serialize(SerializationContext ctx, Utf8FaunaWriter writer)
    {
        writer.WriteStringValue(Unwrap);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current QueryLiteral.
    /// </summary>
    /// <param name="other">The object to compare with the current QueryLiteral.</param>
    /// <returns>true if the specified object is equal to the current QueryLiteral; otherwise, false.</returns>
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

    /// <summary>
    /// The default hash function.
    /// </summary>
    /// <returns>A hash code for the current QueryLiteral.</returns>
    public override int GetHashCode()
    {
        return Unwrap.GetHashCode();
    }

    /// <summary>
    /// Determines whether two specified instances of QueryLiteral are equal.
    /// </summary>
    /// <param name="left">The first QueryLiteral to compare.</param>
    /// <param name="right">The second QueryLiteral to compare.</param>
    /// <returns>true if left and right are equal; otherwise, false.</returns>
    public static bool operator ==(QueryLiteral left, QueryLiteral right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two specified instances of QueryLiteral are not equal.
    /// </summary>
    /// <param name="left">The first QueryLiteral to compare.</param>
    /// <param name="right">The second QueryLiteral to compare.</param>
    /// <returns>true if left and right are not equal; otherwise, false.</returns>
    public static bool operator !=(QueryLiteral left, QueryLiteral right)
    {
        return !Equals(left, right);
    }
}
