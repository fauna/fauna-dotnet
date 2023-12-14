using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Fauna;

internal class ExpressionClosureFinder : ExpressionSwitch<object?>
{
    private static ExpressionClosureFinder _switch = new ExpressionClosureFinder();

    public static object? Find(Expression expr) => _switch.Apply(expr);

    protected override object? ConstantExpr(ConstantExpression expr)
    {
        var ty = expr.Type;
        var compilerGen = ty.GetCustomAttribute<CompilerGeneratedAttribute>() != null;
        // check for the closure class name pattern. see
        // https://stackoverflow.com/questions/2508828/where-to-learn-about-vs-debugger-magic-names/2509524#2509524
        var dcName = ty.Name.StartsWith("<>c__DisplayClass");

        if (compilerGen && dcName) return expr.Value;

        return null;
    }

    protected override object? BinaryExpr(BinaryExpression expr) =>
        Apply(expr.Left) ?? Apply(expr.Right);

    protected override object? CallExpr(MethodCallExpression expr) =>
        (expr.Object is null ? null : Apply(expr.Object)) ?? FirstOf(expr.Arguments);

    protected override object? MemberAccessExpr(MemberExpression expr) => Apply(expr.Expression);

    protected override object? NewArrayExpr(NewArrayExpression expr) => FirstOf(expr.Expressions);

    protected override object? NewExpr(NewExpression expr) => FirstOf(expr.Arguments);

    protected override object? LambdaExpr(LambdaExpression expr) => Apply(expr.Body);

    protected override object? ParameterExpr(ParameterExpression expr) => null;

    protected override object? UnaryExpr(UnaryExpression expr) => Apply(expr.Operand);

    private object? FirstOf(IEnumerable<Expression> exprs)
    {
        foreach (var e in exprs)
        {
            var t = Apply(e);
            if (t != null) return t;
        }

        return null;
    }
}
