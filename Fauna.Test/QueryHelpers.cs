using System.Text;
using Fauna.Mapping;
using Fauna.Serialization;

namespace Fauna.Test;

public static class IQueryFragmentTestExtensions
{
    private static readonly MappingContext ctx = new();

    public static string Serialize(this IQueryFragment fragment)
    {
        using var ms = new MemoryStream();
        using var fw = new Utf8FaunaWriter(ms);
        fragment.Serialize(ctx, fw);
        fw.Flush();
        return Encoding.UTF8.GetString(ms.ToArray());
    }
}
