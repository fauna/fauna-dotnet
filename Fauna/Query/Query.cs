using System.Text;

namespace Fauna;

public abstract class Query : IEquatable<Query>, IQueryFragment
{
    public void Serialize(Stream stream)
    {
        SerializeInternal(stream);
    }

    public string Serialize()
    {
        using var ms = new MemoryStream();
        SerializeInternal(ms);
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    protected abstract void SerializeInternal(Stream stream);

    public abstract override int GetHashCode();

    public abstract override bool Equals(object? otherObject);

    public abstract bool Equals(Query? otherQuery);

    public static Query FQL(ref QueryStringHandler handler)
    {
        return handler.Result();
    }
}
