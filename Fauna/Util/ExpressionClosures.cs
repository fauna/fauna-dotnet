using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Fauna.Util;

internal class ExpressionClosures
{
    public static bool IsClosureType(Type ty)
    {
        var compilerGen = ty.GetCustomAttribute<CompilerGeneratedAttribute>() != null;
        // check for the closure class name pattern. see
        // https://stackoverflow.com/questions/2508828/where-to-learn-about-vs-debugger-magic-names/2509524#2509524
        var dcName = ty.Name.StartsWith("<>c__DisplayClass");

        return compilerGen && dcName;
    }

    public static object[] FindAll(Expression expr)
    {
        var finder = new Finder();
        finder.Visit(expr);
        return finder.closures.ToArray();
    }

    private class Finder : ExpressionVisitor
    {
        // most exprs have one closure at most
        HashSet<object> seen = new HashSet<object>(1);
        public List<object> closures = new List<object>(1);

        protected override Expression VisitConstant(ConstantExpression node)
        {
            // node.Value cannot be null if closure class shape checks out.
            if (IsClosureType(node.Type) && !seen.Contains(node.Value!))
            {
                seen.Add(node.Value!);
                closures.Add(node.Value!);
            }

            return base.VisitConstant(node);
        }
    }
}
