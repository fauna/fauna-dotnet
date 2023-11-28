namespace Fauna;

using System.Runtime.CompilerServices;

[InterpolatedStringHandler]
public ref struct QueryStringHandler
{
    List<Object> fragments;

    public QueryStringHandler(int literalLength, int formattedCount)
    {
        fragments = new List<Object>();
    }

    public void AppendLiteral(string s)
    {
        fragments.Add(s);
    }

    public void AppendFormatted(Query value)
    {
        fragments.Add(value);
    }

    public void AppendFormatted(Object value)
    {
        fragments.Add(new Query.Val(value));
    }

    public Query Result()
    {
        return new Query.Expr(fragments);
    }
}
