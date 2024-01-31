using System.Runtime.CompilerServices;

namespace Fauna;

/// <summary>
/// Provides a mechanism to build FQL query expressions using interpolated strings. This structure collects fragments and literals to construct complex query expressions.
/// </summary>
[InterpolatedStringHandler]
public ref struct QueryStringHandler
{
    List<IQueryFragment> fragments;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryStringHandler"/> struct.
    /// </summary>
    /// <param name="literalLength">The estimated length of the literals in the interpolated string.</param>
    /// <param name="formattedCount">The number of format items in the interpolated string.</param>
    public QueryStringHandler(int literalLength, int formattedCount)
    {
        fragments = new List<IQueryFragment>();
    }

    /// <summary>
    /// Appends a literal string to the query.
    /// </summary>
    /// <param name="value">The literal string to append.</param>
    public void AppendLiteral(string value)
    {
        fragments.Add(new QueryLiteral(value));
    }

    /// <summary>
    /// Appends a formatted value to the query. The value is wrapped as a <see cref="QueryVal"/> or <see cref="QueryExpr"/> depending on its type.
    /// </summary>
    /// <param name="value">The value to append.</param>
    public void AppendFormatted(object? value)
    {
        if (value is QueryExpr expr)
        {
            fragments.Add(expr);
        }
        else
        {
            fragments.Add(new QueryVal(value));
        }
    }

    /// <summary>
    /// Constructs and returns a <see cref="Query"/> instance representing the current state of the handler.
    /// </summary>
    /// <returns>A <see cref="Query"/> instance representing the constructed query fragments.</returns>
    public Query Result()
    {
        return new QueryExpr(fragments);
    }
}
