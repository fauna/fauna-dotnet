using Fauna.Serialization;

namespace Fauna;

public sealed class QueryVal<T> : Query, IQueryFragment
{
    public QueryVal(T v)
    {
        Unwrap = v;
    }

    public T Unwrap { get; }

    public override void Serialize(SerializationContext ctx, Utf8FaunaWriter writer)
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
