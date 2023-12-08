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
            // Determined element type
            Type? elementType = null;
            foreach (Type interfaceType in enumerableValue.GetType().GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    elementType = interfaceType.GetGenericArguments()[0];
                    break;
                }
            }

            if (elementType != null)
            {
                // Create QueryArr<> instance
                Type queryArrType = typeof(QueryArr<>).MakeGenericType(elementType);
                var queryArrInstance = Activator.CreateInstance(queryArrType, enumerableValue);

                if (queryArrInstance == null)
                {
                    throw new InvalidCastException("Failed to create QueryArr instance.");
                }

                // Create QueryVal<QueryArr<>> instance
                Type queryValType = typeof(QueryVal<>).MakeGenericType(queryArrType);
                var queryValInstance = Activator.CreateInstance(queryValType, queryArrInstance);

                if (queryValInstance == null)
                {
                    throw new InvalidCastException("Failed to create QueryVal instance.");
                }

                fragments.Add((IQueryFragment)queryValInstance);
            }
            else
            {
                throw new InvalidOperationException("Unable to determine the element type of the IEnumerable.");
            }
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
