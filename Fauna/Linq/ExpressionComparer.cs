using Fauna.Util;
using System.Linq.Expressions;

namespace Fauna.Linq;

internal class ExpressionComparer : EqualityComparer<Expression>
{
    public static ExpressionComparer Singleton { get; } = new();

    public override bool Equals(Expression? e1, Expression? e2)
    {
        if (ReferenceEquals(e1, e2))
            return true;
        else if (e1 is null || e2 is null)
            return false;

        var sw = new CompareSwitch(e1);
        return sw.Apply(e2);
    }

    public override int GetHashCode(Expression e)
    {
        var sw = new HashCodeSwitch();
        return sw.Apply(e);
    }

    private class CompareSwitch : ExpressionCompareSwitch
    {
        public CompareSwitch(Expression lhs) : base(lhs) { }

        protected override bool ConstantExpr(ConstantExpression rhs)
        {
            var lhs = _lhs as ConstantExpression;
            if (lhs is null) return false;

            if (lhs.Type != rhs.Type) return false;

            // we want to consider two exprs equal even if closure instances
            // differ, since we parameterize on these when building the FQL
            // generator expr.
            if (lhs.Type.IsClosureType()) return true;

            return lhs.Value == rhs.Value;
        }
    }

    private class HashCodeSwitch : ExpressionHashCodeSwitch
    {
        protected override int ConstantExpr(ConstantExpression e) =>
            e.Type.IsClosureType() ?
                Base(e) * e.Type.GetHashCode() :
                base.ConstantExpr(e);
    }
}
