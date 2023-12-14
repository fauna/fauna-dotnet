namespace Fauna;

using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

public interface LinqModule
{
    public Fauna.Types.Module Module { get; }
    public string Name { get => Module.Name; }
}

public class LinqCollection<TDoc> : LinqQuery<TDoc>, LinqModule
{
    public Fauna.Types.Module Module { get; }

    public LinqCollection(string name)
    {
        Module = new Fauna.Types.Module(name);
    }
}


public class LinqQuery<T> : IQueryable<T>
{
    Expression _expr;
    LinqQueryProvider _provider;

    public LinqQuery()
    {
        _expr = Expression.Constant(this);
        _provider = new LinqQueryProvider();
    }

    public LinqQuery(Expression expression)
    {
        _expr = expression;
        _provider = new LinqQueryProvider();
    }

    #region IQueryable members

    Type IQueryable.ElementType => typeof(T);
    Expression IQueryable.Expression => _expr;
    IQueryProvider IQueryable.Provider => _provider;

    #endregion

    #region IEnumerable members

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    #endregion
}

public class LinqQueryProvider : IQueryProvider
{
    IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        return new LinqQuery<TElement>(expression);
    }

    IQueryable IQueryProvider.CreateQuery(Expression expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        // FIXME(matt) this reflection is not accurate.
        var elType = expression.Type;
        var qtype = typeof(LinqQuery<>).MakeGenericType(elType);
        return (IQueryable)Activator.CreateInstance(qtype, expression);
    }

    TResult IQueryProvider.Execute<TResult>(Expression expression)
    {
        var fql = LinqQueryBuilder.Build(expression);
        Console.WriteLine(fql);
        throw new NotImplementedException();
    }

    object? IQueryProvider.Execute(Expression expression)
    {
        var fql = LinqQueryBuilder.Build(expression);
        Console.WriteLine(fql);
        throw new NotImplementedException();
    }
}
