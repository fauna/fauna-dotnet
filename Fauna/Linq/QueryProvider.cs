using System.Linq.Expressions;

namespace Fauna.Linq;

public class QueryProvider : IQueryProvider
{
    private DataContext _ctx;

    public QueryProvider(DataContext ctx)
    {
        _ctx = ctx;
    }

    #region IQueryProvider members

    IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
    {
        if (expression is null)
            throw new ArgumentNullException(nameof(expression));

        return new QuerySource<TElement>(expression, this);
    }

    IQueryable IQueryProvider.CreateQuery(Expression expression)
    {
        if (expression is null)
            throw new ArgumentNullException(nameof(expression));

        // TODO(matt) widen acceptable types here and/or validate
        var exprTy = expression.Type;

        if (exprTy.GenericTypeArguments.Length != 1)
            throw new ArgumentException($"{nameof(expression)} type is invalid: {exprTy}");

        var elemTy = exprTy.GenericTypeArguments[0];
        var qTy = typeof(QuerySource<>).MakeGenericType(elemTy);

        return (IQueryable)Activator.CreateInstance(qTy, expression)!;
    }

    TResult IQueryProvider.Execute<TResult>(Expression expression)
    {
        throw new NotImplementedException();
    }

    object? IQueryProvider.Execute(Expression expression)
    {
        throw new NotImplementedException();
    }

    #endregion
}
