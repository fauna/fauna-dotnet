using System.Text;

namespace Fauna;

/// <summary>
/// Represents the base interface for a query fragment used for FQL query construction.
/// </summary>
public interface IQueryFragment
{
    /// <summary>
    /// Serializes the query fragment into a string format.
    /// </summary>
    /// <returns>A string representation of the query fragment.</returns>
    void Serialize(Stream stream);
    // string Serialize();
}

public static class IQueryFragmentExtensions
{
    public static string Serialize(this IQueryFragment fragment)
    {
        using var ms = new MemoryStream();
        fragment.Serialize(ms);
        ms.Flush();
        return Encoding.UTF8.GetString(ms.ToArray());
    }
}
