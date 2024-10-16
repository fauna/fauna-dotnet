namespace Fauna.Test.Performance;

/// <summary>
/// Extension methods for performance test queries
/// </summary>
internal static class TestQueryExtensions
{
    /// <summary>
    /// Get a <see cref="Query"/> composited from the given string query fragments
    /// </summary>
    /// <param name="queryParts">List of strings from which to create the query</param>
    /// <returns>A <see cref="QueryExpr"/> representing the composite query</returns>
    public static Query GetCompositedQueryFromParts(this List<string> queryParts)
    {
        var handler = new QueryStringHandler(0, 0);

        foreach (var part in queryParts)
        {
            handler.AppendLiteral(part);
        }

        return Query.FQL(ref handler);
    }
}
