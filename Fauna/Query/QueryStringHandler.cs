namespace Fauna;

using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Fauna.Query;

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

    public void AppendFormatted<T>(T value)
    {
        if (((value is IEnumerable && value is not string) || value is IDictionary) && ContainsExpr(value))
        {
            throw new InvalidOperationException("FQL parameter cannot contain a query expression within List, Array, or Dictionary elements.");
        }

        fragments.Add(new Query.Val<T>(value));
    }

    public void AppendFormatted(Object value)
    {
        fragments.Add(new Query.Val<Object>(value));
    }

    public Query Result()
    {
        return new Query.Expr(fragments);
    }

    private static bool ContainsExpr<T>(T value)
    {
        if (value is IEnumerable enumerable && value is not string)
        {
            if (value is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (entry.Value is Expr)
                    {
                        return true;
                    }
                }
            }
            else
            {
                foreach (var item in enumerable)
                {
                    if (item is Expr)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
