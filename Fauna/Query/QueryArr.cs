using System.Collections;
using System.Collections.ObjectModel;
using Fauna.Serialization;

namespace Fauna;

internal sealed class QueryArr<T> : Query, IQueryFragment, IEnumerable<T>
{
    public QueryArr(IEnumerable<T> v)
    {
        if (v == null)
        {
            throw new ArgumentNullException(nameof(v), "Value cannot be null.");
        }

        Unwrap = new ReadOnlyCollection<T>(v.ToList());
    }

    public ReadOnlyCollection<T> Unwrap { get; }

    public int Count => Unwrap.Count;

    public T this[int index] => Unwrap[index];

    public override void Serialize(Utf8FaunaWriter writer)
    {
        throw new NotImplementedException();
    }

    public override bool Equals(Query? o)
    {
        return o is QueryArr<T> other && Unwrap.SequenceEqual(other.Unwrap);
    }

    public override bool Equals(object? otherObject)
    {
        return Equals(otherObject as Query);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            for (int i = 0; i < Unwrap.Count; i++)
            {
                T item = Unwrap[i];
                hash = hash * 31 + (item?.GetHashCode() ?? 0);
            }
            return hash;
        }
    }

    public override string ToString()
    {
        return $"QueryArr({string.Join(", ", Unwrap)})";
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Unwrap.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
