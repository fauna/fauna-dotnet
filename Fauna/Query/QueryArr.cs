using Fauna.Mapping;
using Fauna.Serialization;

namespace Fauna;

/// <summary>
/// Represents an array of FQL queries.
/// </summary>
internal sealed class QueryArr : Query, IQueryFragment
{
    /// <summary>
    /// Gets the value of the specified type represented in the query.
    /// </summary>
    public IEnumerable<Query> Unwrap { get; }

    /// <summary>
    /// Initializes a new instance of the QueryArr class with the specified value.
    /// </summary>
    /// <param name="v">The value of the specified type to be represented in the query.</param>
    public QueryArr(IEnumerable<Query> v)
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
        writer.WriteFieldName("array");
        writer.WriteStartArray();
        foreach (Query t in Unwrap)
        {
            var ser = Serializer.Generate(ctx, t.GetType());
            ser.Serialize(ctx, writer, t);
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    /// <summary>
    /// Determines whether the specified QueryArr is equal to the current QueryArr.
    /// </summary>
    /// <param name="o">The QueryArr to compare with the current QueryArr.</param>
    /// <returns>true if the specified QueryArr is equal to the current QueryArr; otherwise, false.</returns>
    public override bool Equals(Query? o)
    {
        return o is QueryArr other && Unwrap.SequenceEqual(other.Unwrap);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current QueryArr.
    /// </summary>
    /// <param name="otherObject">The object to compare with the current QueryArr.</param>
    /// <returns>true if the specified object is equal to the current QueryArr; otherwise, false.</returns>
    public override bool Equals(object? otherObject)
    {
        return Equals(otherObject as QueryArr);
    }

    /// <summary>
    /// The default hash function.
    /// </summary>
    /// <returns>A hash code for the current QueryArr.</returns>
    public override int GetHashCode()
    {
        return Unwrap.GetHashCode();
    }

    /// <summary>
    /// Returns a string that represents the current QueryArr.
    /// </summary>
    /// <returns>A string that represents the current QueryArr.</returns>
    public override string ToString()
    {
        return $"QueryArr({string.Join(", ", Unwrap)})";
    }

}
