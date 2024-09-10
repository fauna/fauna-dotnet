using System.Collections;
using Fauna.Mapping;
using Fauna.Serialization;

namespace Fauna;

internal sealed class QueryArr : Query, IQueryFragment
{
    public QueryArr(IEnumerable<object?> v)
    {
        if (v == null)
        {
            throw new ArgumentNullException(nameof(v), "Value cannot be null.");
        }

        Unwrap = v;
    }

    public IEnumerable<object?> Unwrap { get; }

    public override void Serialize(MappingContext ctx, Utf8FaunaWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteFieldName("array");
        writer.WriteStartArray();
        foreach (object? t in Unwrap)
        {
            if (t is IQueryFragment frag)
            {
                frag.Serialize(ctx, writer);
            }
            else
            {
                var ser = t is not null ? Serializer.Generate(ctx, t.GetType()) : DynamicSerializer.Singleton;
                ser.Serialize(ctx, writer, t);
            }
        }
        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    public override bool Equals(Query? o)
    {
        return o is QueryArr other && Unwrap.SequenceEqual(other.Unwrap);
    }

    public override bool Equals(object? otherObject)
    {
        return Equals(otherObject as Query);
    }

    public override int GetHashCode()
    {
        return Unwrap.GetHashCode();
    }

    public override string ToString()
    {
        return $"QueryArr({string.Join(", ", Unwrap)})";
    }

}
