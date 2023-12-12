using System.Runtime.CompilerServices;

namespace Fauna;
[InterpolatedStringHandler]
public ref struct QueryStringHandler
{
    List<IQueryFragment> fragments;

    public QueryStringHandler(int literalLength, int formattedCount)
    {
        fragments = new List<IQueryFragment>();
    }

    public void AppendLiteral(string value)
    {
        fragments.Add(new QueryLiteral(value));
    }

    public void AppendFormatted<T>(T value)
    {
        if (value is QueryExpr expr)
        {
            fragments.Add(expr);
        }
        else
        {
            fragments.Add(new QueryVal<T>(value));
        }
    }

    public void AppendFormatted(Object value)
    {
        fragments.Add(new QueryVal<Object>(value));
    }

    public Query Result()
    {
        return new QueryExpr(fragments);
    }
}
