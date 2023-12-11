using System.Collections;
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
        else if (value is IEnumerable enumerableValue && !(value is string))
        {
            // TODO: Implement support for QueryArr when Fauna is ready to handle these.
            throw new NotSupportedException("IEnumerable is not supported in Fauna queries yet.");
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
