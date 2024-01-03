using Fauna.Serialization;

namespace Fauna;

/// <summary>
/// Represents the abstract base class for constructing FQL queries.
/// </summary>

public abstract class Query : IEquatable<Query>, IQueryFragment
{
    /// <summary>
    /// Serializes the query into the provided stream.
    /// </summary>
    /// <param name="ctx">The context to be used during serialization.</param>
    /// <param name="writer">The writer to which the query is serialized.</param>
    public abstract void Serialize(SerializationContext ctx, Utf8FaunaWriter writer);

    /// <summary>
    /// Returns a hash code for the current query.
    /// </summary>
    /// <returns>A hash code for the current query.</returns>
    public abstract override int GetHashCode();

    /// <summary>
    /// Determines whether the specified object is equal to the current query.
    /// </summary>
    /// <param name="otherObject">The object to compare with the current query.</param>
    /// <returns>true if the specified object is equal to the current query; otherwise, false.</returns>
    public abstract override bool Equals(object? otherObject);

    /// <summary>
    /// Determines whether the specified Query is equal to the current query.
    /// </summary>
    /// <param name="otherQuery">The Query to compare with the current query.</param>
    /// <returns>true if the specified Query is equal to the current query; otherwise, false.</returns>
    public abstract bool Equals(Query? otherQuery);

    /// <summary>
    /// Constructs an FQL query using the specified QueryStringHandler.
    /// </summary>
    /// <param name="handler">The QueryStringHandler that contains the query fragments.</param>
    /// <returns>A Query instance constructed from the handler.</returns>
    public static Query FQL(ref QueryStringHandler handler)
    {
        return handler.Result();
    }
}
