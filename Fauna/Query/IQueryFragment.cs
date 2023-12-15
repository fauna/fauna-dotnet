using System.Text;
using Fauna.Serialization;

namespace Fauna;

/// <summary>
/// Represents the base interface for a query fragment used for FQL query construction.
/// </summary>
public interface IQueryFragment
{
    /// <summary>
    /// Serializes the query fragment into the provided stream.
    /// </summary>
    /// <param name="writer">The writer to which the query fragment is serialized.</param>
    void Serialize(Utf8FaunaWriter writer);
}

public static class IQueryFragmentExtensions
{
    public static string Serialize(this IQueryFragment fragment)
    {
        using var ms = new MemoryStream();
        using var fw = new Utf8FaunaWriter(ms);
        fragment.Serialize(fw);
        fw.Flush();
        return Encoding.UTF8.GetString(ms.ToArray());
    }
}
