using Fauna.Mapping;
using Fauna.Serialization;
using System.Text;

namespace Fauna;

/// <summary>
/// Represents the base interface for a query fragment used for FQL query construction.
/// </summary>
public interface IQueryFragment
{
    /// <summary>
    /// Serializes the query fragment into the provided stream.
    /// </summary>
    /// <param name="ctx">The context to be used during serialization.</param>
    /// <param name="writer">The writer to which the query fragment is serialized.</param>
    void Serialize(MappingContext ctx, Utf8FaunaWriter writer);
}

/// <summary>
/// Provides extension methods for the <see cref="IQueryFragment"/> interface to enhance its functionality,
/// allowing for more flexible serialization options.
/// </summary>
public static class IQueryFragmentExtensions
{
    /// <summary>
    /// Serializes the query fragment to a string using UTF-8 encoding.
    /// </summary>
    /// <param name="fragment">The query fragment to serialize.</param>
    /// <returns>A string representation of the serialized query fragment.</returns>
    public static string Serialize(this IQueryFragment fragment, MappingContext ctx)
    {
        using var ms = new MemoryStream();
        using var fw = new Utf8FaunaWriter(ms);
        fragment.Serialize(ctx, fw);
        fw.Flush();
        return Encoding.UTF8.GetString(ms.ToArray());
    }
}
