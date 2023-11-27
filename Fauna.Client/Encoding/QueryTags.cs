namespace Fauna.Encoding;

public class QueryTags
{
    public static string Encode(Dictionary<string, string> tags)
    {
        return string.Join(",", tags.Select(entry => entry.Key + "=" + entry.Value));
    }
}
