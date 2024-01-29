using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Fauna.Linq;

public abstract class QuerySource
{
    internal abstract void SetProvider(QueryProvider provider);
}

public class QuerySource<T> : QuerySource, IQueryable<T>
{
    [AllowNull]
    internal Expression _expr;
    [AllowNull]
    internal QueryProvider _provider;

    public QuerySource(Expression expr, QueryProvider provider)
    {
        _expr = expr;
        _provider = provider;
    }

    // Collection/Index DSLs are allowed to set _expr and _provider in their own
    // constructors, so they use this base one.
    internal QuerySource() { }

    internal override void SetProvider(QueryProvider provider)
    {
        _provider = provider;
    }

    #region IQueryable members

    Type IQueryable.ElementType => typeof(T);
    Expression IQueryable.Expression => _expr;
    IQueryProvider IQueryable.Provider => _provider;

    #endregion

    #region IEnumerable members

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    #endregion
}
