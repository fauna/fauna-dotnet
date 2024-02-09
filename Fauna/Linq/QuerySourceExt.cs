using System.Linq.Expressions;
using System.Reflection;
using Fauna.Linq;

namespace Fauna;

public static class QuerySourceExt
{
    // public static R Aggregate<T, A, R>(this IQuerySource<T> src, A seed, Expression<Func<A, T, A>> acc, Expression<Func<A, R>> selector) =>
    //     src.Call<R>(new Func<IQuerySource<T>, A, Expression<Func<A, T, A>>, Expression<Func<A, R>>, R>(Aggregate).Method,
    //                 Expression.Constant(seed), acc, selector);

    public static bool All<T>(this IQuerySource<T> src, Expression<Func<T, bool>> predicate) =>
        src.Call<bool>(new Func<IQuerySource<T>, Expression<Func<T, bool>>, bool>(All).Method,
                       predicate);

    public static bool Any(this IQuerySource src) =>
        src.Call<bool>(new Func<IQuerySource, bool>(Any).Method);

    public static bool Any<T>(this IQuerySource<T> src, Expression<Func<T, bool>> predicate) =>
        src.Call<bool>(new Func<IQuerySource<T>, Expression<Func<T, bool>>, bool>(Any).Method,
                       predicate);

    public static int Count(this IQuerySource src) =>
        src.Call<int>(new Func<IQuerySource, int>(Count).Method);

    public static int Count<T>(this IQuerySource<T> src, Expression<Func<T, bool>> predicate) =>
        src.Call<int>(new Func<IQuerySource<T>, Expression<Func<T, bool>>, int>(Count).Method,
        predicate);

    public static T First<T>(this IQuerySource<T> src) =>
        src.Call<T>(new Func<IQuerySource<T>, T>(First).Method);

    public static T First<T>(this IQuerySource<T> src, Expression<Func<T, bool>> predicate) =>
        src.Call<T>(new Func<IQuerySource<T>, Expression<Func<T, bool>>, T>(First).Method,
        predicate);

    public static T FirstOrDefault<T>(this IQuerySource<T> src) =>
        src.Call<T>(new Func<IQuerySource<T>, T>(FirstOrDefault).Method);

    public static T FirstOrDefault<T>(this IQuerySource<T> src, Expression<Func<T, bool>> predicate) =>
        src.Call<T>(new Func<IQuerySource<T>, Expression<Func<T, bool>>, T>(FirstOrDefault).Method,
        predicate);

    // not IQueryable
    // public static R Fold<T, R>(this IQuerySource<T> src, R seed, Expression<Func<R, T, R>> acc) =>
    //     src.Call<R>(new Func<IQuerySource<T>, R, Expression<Func<R, T, R>>, R>(Fold).Method,
    //     Expression.Constant(seed), acc);

    public static T Last<T>(this IQuerySource<T> src) =>
        src.Call<T>(new Func<IQuerySource<T>, T>(Last).Method);

    public static T Last<T>(this IQuerySource<T> src, Expression<Func<T, bool>> predicate) =>
        src.Call<T>(new Func<IQuerySource<T>, Expression<Func<T, bool>>, T>(Last).Method,
        predicate);

    public static T LastOrDefault<T>(this IQuerySource<T> src) =>
        src.Call<T>(new Func<IQuerySource<T>, T>(LastOrDefault).Method);

    public static T LastOrDefault<T>(this IQuerySource<T> src, Expression<Func<T, bool>> predicate) =>
        src.Call<T>(new Func<IQuerySource<T>, Expression<Func<T, bool>>, T>(LastOrDefault).Method,
        predicate);

    // public static long LongCount(this IQuerySource src) =>
    //     src.Call<long>(new Func<IQuerySource, long>(LongCount).Method);

    // public static long LongCount<T>(this IQuerySource<T> src, Expression<Func<T, bool>> predicate) =>
    //     src.Call<long>(new Func<IQuerySource<T>, Expression<Func<T, bool>>, long>(LongCount).Method,
    //     predicate);

    public static T Max<T>(this IQuerySource<T> src) =>
        src.Call<T>(new Func<IQuerySource<T>, T>(Max).Method);

    public static R Max<T, R>(this IQuerySource<T> src, Expression<Func<T, R>> selector) =>
        src.Call<R>(new Func<IQuerySource<T>, Expression<Func<T, R>>, R>(Max).Method,
        selector);

    // public static T MaxBy<T, K>(this IQuerySource<T> src, Expression<Func<T, K>> keySelector) =>
    //     src.Call<T>(new Func<IQuerySource<T>, Expression<Func<T, K>>, T>(MaxBy).Method,
    //     keySelector);

    public static T Min<T>(this IQuerySource<T> src) =>
        src.Call<T>(new Func<IQuerySource<T>, T>(Min).Method);

    public static R Min<T, R>(this IQuerySource<T> src, Expression<Func<T, R>> selector) =>
        src.Call<R>(new Func<IQuerySource<T>, Expression<Func<T, R>>, R>(Min).Method,
                    selector);

    // public static T MinBy<T, K>(this IQuerySource<T> src, Expression<Func<T, K>> keySelector) =>
    //     src.Call<T>(new Func<IQuerySource<T>, Expression<Func<T, K>>, T>(MinBy).Method,
    //     keySelector);

    // public static T Single<T>(this IQuerySource<T> src) =>
    //     src.Call<T>(new Func<IQuerySource<T>, T>(Single).Method);

    // public static T Single<T>(this IQuerySource<T> src, Expression<Func<T, bool>> predicate) =>
    //     src.Call<T>(new Func<IQuerySource<T>, Expression<Func<T, bool>>, T>(Single).Method,
    //                 predicate);

    // public static int Average<T>(this IQuerySource<T> src, Expression<Func<T, int>> selector) =>
    //     src.Call<int>(new Func<IQuerySource<T>, Expression<Func<T, int>>, int>(Average).Method, selector);
    // public static int Average(this IQuerySource<int> src) =>
    //     src.Call<int>(new Func<IQuerySource<int>, int>(Average).Method);
    // public static int Sum<T>(this IQuerySource<T> src, Expression<Func<T, int>> selector) =>
    //     src.Call<int>(new Func<IQuerySource<T>, Expression<Func<T, int>>, int>(Sum).Method, selector);
    public static int Sum(this IQuerySource<int> src) =>
        src.Call<int>(new Func<IQuerySource<int>, int>(Sum).Method);
    // public static int? Average<T>(this IQuerySource<T> src, Expression<Func<T, int?>> selector) =>
    //     src.Call<int?>(new Func<IQuerySource<T>, Expression<Func<T, int?>>, int?>(Average).Method, selector);
    // public static int? Average(this IQuerySource<int?> src) =>
    //     src.Call<int?>(new Func<IQuerySource<int?>, int?>(Average).Method);
    // public static int? Sum<T>(this IQuerySource<T> src, Expression<Func<T, int?>> selector) =>
    //     src.Call<int?>(new Func<IQuerySource<T>, Expression<Func<T, int?>>, int?>(Sum).Method, selector);
    public static int? Sum(this IQuerySource<int?> src) =>
        src.Call<int?>(new Func<IQuerySource<int?>, int?>(Sum).Method);
    // public static long Average<T>(this IQuerySource<T> src, Expression<Func<T, long>> selector) =>
    //     src.Call<long>(new Func<IQuerySource<T>, Expression<Func<T, long>>, long>(Average).Method, selector);
    // public static long Average(this IQuerySource<long> src) =>
    //     src.Call<long>(new Func<IQuerySource<long>, long>(Average).Method);
    // public static long Sum<T>(this IQuerySource<T> src, Expression<Func<T, long>> selector) =>
    //     src.Call<long>(new Func<IQuerySource<T>, Expression<Func<T, long>>, long>(Sum).Method, selector);
    public static long Sum(this IQuerySource<long> src) =>
        src.Call<long>(new Func<IQuerySource<long>, long>(Sum).Method);
    // public static long? Average<T>(this IQuerySource<T> src, Expression<Func<T, long?>> selector) =>
    //     src.Call<long?>(new Func<IQuerySource<T>, Expression<Func<T, long?>>, long?>(Average).Method, selector);
    // public static long? Average(this IQuerySource<long?> src) =>
    //     src.Call<long?>(new Func<IQuerySource<long?>, long?>(Average).Method);
    // public static long? Sum<T>(this IQuerySource<T> src, Expression<Func<T, long?>> selector) =>
    //     src.Call<long?>(new Func<IQuerySource<T>, Expression<Func<T, long?>>, long?>(Sum).Method, selector);
    public static long? Sum(this IQuerySource<long?> src) =>
        src.Call<long?>(new Func<IQuerySource<long?>, long?>(Sum).Method);
    // public static float Average<T>(this IQuerySource<T> src, Expression<Func<T, float>> selector) =>
    //     src.Call<float>(new Func<IQuerySource<T>, Expression<Func<T, float>>, float>(Average).Method, selector);
    // public static float Average(this IQuerySource<float> src) =>
    //     src.Call<float>(new Func<IQuerySource<float>, float>(Average).Method);
    // public static float Sum<T>(this IQuerySource<T> src, Expression<Func<T, float>> selector) =>
    //     src.Call<float>(new Func<IQuerySource<T>, Expression<Func<T, float>>, float>(Sum).Method, selector);
    public static float Sum(this IQuerySource<float> src) =>
        src.Call<float>(new Func<IQuerySource<float>, float>(Sum).Method);
    // public static float? Average<T>(this IQuerySource<T> src, Expression<Func<T, float?>> selector) =>
    //     src.Call<float?>(new Func<IQuerySource<T>, Expression<Func<T, float?>>, float?>(Average).Method, selector);
    // public static float? Average(this IQuerySource<float?> src) =>
    //     src.Call<float?>(new Func<IQuerySource<float?>, float?>(Average).Method);
    // public static float? Sum<T>(this IQuerySource<T> src, Expression<Func<T, float?>> selector) =>
    //     src.Call<float?>(new Func<IQuerySource<T>, Expression<Func<T, float?>>, float?>(Sum).Method, selector);
    public static float? Sum(this IQuerySource<float?> src) =>
        src.Call<float?>(new Func<IQuerySource<float?>, float?>(Sum).Method);
    // public static double Average<T>(this IQuerySource<T> src, Expression<Func<T, double>> selector) =>
    //     src.Call<double>(new Func<IQuerySource<T>, Expression<Func<T, double>>, double>(Average).Method, selector);
    // public static double Average(this IQuerySource<double> src) =>
    //     src.Call<double>(new Func<IQuerySource<double>, double>(Average).Method);
    // public static double Sum<T>(this IQuerySource<T> src, Expression<Func<T, double>> selector) =>
    //     src.Call<double>(new Func<IQuerySource<T>, Expression<Func<T, double>>, double>(Sum).Method, selector);
    public static double Sum(this IQuerySource<double> src) =>
        src.Call<double>(new Func<IQuerySource<double>, double>(Sum).Method);
    // public static double? Average<T>(this IQuerySource<T> src, Expression<Func<T, double?>> selector) =>
    //     src.Call<double?>(new Func<IQuerySource<T>, Expression<Func<T, double?>>, double?>(Average).Method, selector);
    // public static double? Average(this IQuerySource<double?> src) =>
    //     src.Call<double?>(new Func<IQuerySource<double?>, double?>(Average).Method);
    // public static double? Sum<T>(this IQuerySource<T> src, Expression<Func<T, double?>> selector) =>
    //     src.Call<double?>(new Func<IQuerySource<T>, Expression<Func<T, double?>>, double?>(Sum).Method, selector);
    public static double? Sum(this IQuerySource<double?> src) =>
        src.Call<double?>(new Func<IQuerySource<double?>, double?>(Sum).Method);
    // public static decimal Average<T>(this IQuerySource<T> src, Expression<Func<T, decimal>> selector) =>
    //     src.Call<decimal>(new Func<IQuerySource<T>, Expression<Func<T, decimal>>, decimal>(Average).Method, selector);
    // public static decimal Average(this IQuerySource<decimal> src) =>
    //     src.Call<decimal>(new Func<IQuerySource<decimal>, decimal>(Average).Method);
    // public static decimal Sum<T>(this IQuerySource<T> src, Expression<Func<T, decimal>> selector) =>
    //     src.Call<decimal>(new Func<IQuerySource<T>, Expression<Func<T, decimal>>, decimal>(Sum).Method, selector);
    public static decimal Sum(this IQuerySource<decimal> src) =>
        src.Call<decimal>(new Func<IQuerySource<decimal>, decimal>(Sum).Method);
    // public static decimal? Average<T>(this IQuerySource<T> src, Expression<Func<T, decimal?>> selector) =>
    //     src.Call<decimal?>(new Func<IQuerySource<T>, Expression<Func<T, decimal?>>, decimal?>(Average).Method, selector);
    // public static decimal? Average(this IQuerySource<decimal?> src) =>
    //     src.Call<decimal?>(new Func<IQuerySource<decimal?>, decimal?>(Average).Method);
    // public static decimal? Sum<T>(this IQuerySource<T> src, Expression<Func<T, decimal?>> selector) =>
    //     src.Call<decimal?>(new Func<IQuerySource<T>, Expression<Func<T, decimal?>>, decimal?>(Sum).Method, selector);
    public static decimal? Sum(this IQuerySource<decimal?> src) =>
        src.Call<decimal?>(new Func<IQuerySource<decimal?>, decimal?>(Sum).Method);

    public static IQuerySource<T> Distinct<T>(this IQuerySource<T> src) =>
        src.Chain<T>(new Func<IQuerySource<T>, IQuerySource<T>>(Distinct).Method);

    public static IQuerySource<T> Order<T>(this IQuerySource<T> src) =>
        src.Chain<T>(new Func<IQuerySource<T>, IQuerySource<T>>(Order).Method);

    public static IQuerySource<T> OrderBy<T, K>(this IQuerySource<T> src, Expression<Func<T, K>> keySelector) =>
        src.Chain<T>(new Func<IQuerySource<T>, Expression<Func<T, K>>, IQuerySource<T>>(OrderBy).Method,
                     keySelector);

    public static IQuerySource<T> OrderDescending<T>(this IQuerySource<T> src) =>
        src.Chain<T>(new Func<IQuerySource<T>, IQuerySource<T>>(OrderDescending).Method);

    public static IQuerySource<T> OrderByDescending<T, K>(this IQuerySource<T> src, Expression<Func<T, K>> keySelector) =>
        src.Chain<T>(new Func<IQuerySource<T>, Expression<Func<T, K>>, IQuerySource<T>>(OrderByDescending).Method,
                     keySelector);

    public static IQuerySource<T> Reverse<T>(this IQuerySource<T> src) =>
        src.Chain<T>(new Func<IQuerySource<T>, IQuerySource<T>>(Reverse).Method);

    public static IQuerySource<R> Select<T, R>(this IQuerySource<T> src, Expression<Func<T, R>> selector) =>
        src.Chain<R>(new Func<IQuerySource<T>, Expression<Func<T, R>>, IQuerySource<R>>(Select).Method,
                     selector);

    // public static IQuerySource<R> SelectMany<T, R>(this IQuerySource<T> src, Expression<Func<T, IQuerySource<R>>> selector) =>
    //     src.Chain<R>(new Func<IQuerySource<T>, Expression<Func<T, IQuerySource<R>>>, IQuerySource<R>>(SelectMany).Method,
    //                  selector);

    public static IQuerySource<T> Skip<T>(this IQuerySource<T> src, int count) =>
        src.Chain<T>(new Func<IQuerySource<T>, int, IQuerySource<T>>(Skip).Method,
                     Expression.Constant(count));

    public static IQuerySource<T> Take<T>(this IQuerySource<T> src, int count) =>
        src.Chain<T>(new Func<IQuerySource<T>, int, IQuerySource<T>>(Take).Method,
                     Expression.Constant(count));

    public static IQuerySource<T> Where<T>(this IQuerySource<T> src, Expression<Func<T, bool>> predicate) =>
        src.Chain<T>(new Func<IQuerySource<T>, Expression<Func<T, bool>>, IQuerySource<T>>(Where).Method,
                     predicate);

    // helpers

    private static Expression Expr(this IQuerySource src) =>
        ((QuerySource)src).Expr;

    private static IQuerySource<TResult> WithExpr<TResult>(this IQuerySource src, Expression expr) =>
        new QuerySource<TResult>(expr, ((QuerySource)src).Ctx);

    private static TResult Execute<TResult>(this IQuerySource src, Expression expr) =>
        ((QuerySource)src).Execute<TResult>(expr);

    private static IQuerySource<TResult> Chain<TResult>(this IQuerySource src, MethodInfo method, Expression a1) =>
        src.WithExpr<TResult>(Expression.Call(null, method, new Expression[] { src.Expr(), a1 }));
    private static IQuerySource<TResult> Chain<TResult>(this IQuerySource src, MethodInfo method) =>
        src.WithExpr<TResult>(Expression.Call(null, method, new Expression[] { src.Expr() }));

    private static TResult Call<TResult>(this IQuerySource src, MethodInfo method, Expression a1, Expression a2, Expression a3) =>
        src.Execute<TResult>(Expression.Call(null, method, new Expression[] { src.Expr(), a1, a2, a3 }));
    private static TResult Call<TResult>(this IQuerySource src, MethodInfo method, Expression a1, Expression a2) =>
        src.Execute<TResult>(Expression.Call(null, method, new Expression[] { src.Expr(), a1, a2 }));
    private static TResult Call<TResult>(this IQuerySource src, MethodInfo method, Expression a1) =>
        src.Execute<TResult>(Expression.Call(null, method, new Expression[] { src.Expr(), a1 }));
    private static TResult Call<TResult>(this IQuerySource src, MethodInfo method) =>
        src.Execute<TResult>(Expression.Call(null, method, new Expression[] { src.Expr() }));
}
