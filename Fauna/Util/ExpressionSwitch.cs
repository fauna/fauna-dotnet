using System.Linq.Expressions;

namespace Fauna;

internal class ExpressionSwitch<TResult>
{
    // if true, will transparently handle certain node types (Quote, Convert, etc.)
    protected virtual bool Simplified { get => true; }

    // Apply this switch to an expression
    public TResult Apply(Expression? expr) => ApplyImpl(expr);

    protected virtual TResult ApplyDefault(Expression? expr)
        => throw new NotSupportedException($"Unsupported expression: {expr}");

    protected virtual TResult BinaryExpr(BinaryExpression expr) => ApplyDefault(expr);
    protected virtual TResult BlockExpr(BlockExpression expr) => ApplyDefault(expr);
    protected virtual TResult ConditionalExpr(ConditionalExpression expr) => ApplyDefault(expr);
    protected virtual TResult CallExpr(MethodCallExpression expr) => ApplyDefault(expr);
    protected virtual TResult ConstantExpr(ConstantExpression expr) => ApplyDefault(expr);
    protected virtual TResult DebugInfoExpr(DebugInfoExpression expr) => ApplyDefault(expr);
    protected virtual TResult DefaultExpr(DefaultExpression expr) => ApplyDefault(expr);
    protected virtual TResult DynamicExpr(DynamicExpression expr) => ApplyDefault(expr);
    protected virtual TResult GotoExpr(GotoExpression expr) => ApplyDefault(expr);
    protected virtual TResult IndexExpr(IndexExpression expr) => ApplyDefault(expr);
    protected virtual TResult InvokeExpr(InvocationExpression expr) => ApplyDefault(expr);
    protected virtual TResult LabelExpr(LabelExpression expr) => ApplyDefault(expr);
    protected virtual TResult LambdaExpr(LambdaExpression expr) => ApplyDefault(expr);
    protected virtual TResult LoopExpr(LoopExpression expr) => ApplyDefault(expr);
    protected virtual TResult ListInitExpr(ListInitExpression expr) => ApplyDefault(expr);
    protected virtual TResult MemberAccessExpr(MemberExpression expr) => ApplyDefault(expr);
    protected virtual TResult MemberInitExpr(MemberInitExpression expr) => ApplyDefault(expr);
    protected virtual TResult NewExpr(NewExpression expr) => ApplyDefault(expr);
    protected virtual TResult NewArrayExpr(NewArrayExpression expr) => ApplyDefault(expr);
    protected virtual TResult ParameterExpr(ParameterExpression expr) => ApplyDefault(expr);
    protected virtual TResult RuntimeVariablesExpr(RuntimeVariablesExpression expr) => ApplyDefault(expr);
    protected virtual TResult SwitchExpr(SwitchExpression expr) => ApplyDefault(expr);
    protected virtual TResult TryExpr(TryExpression expr) => ApplyDefault(expr);
    protected virtual TResult TypeBinaryExpr(TypeBinaryExpression expr) => ApplyDefault(expr);
    protected virtual TResult UnaryExpr(UnaryExpression expr) => ApplyDefault(expr);

    public TResult ApplyImpl(Expression? expr)
    {
        if (expr is null) return ApplyDefault(expr);

        return expr.NodeType switch
        {
            ExpressionType.Add or
            ExpressionType.AddAssign or
            ExpressionType.AddAssignChecked or
            ExpressionType.AddChecked or
            ExpressionType.And or
            ExpressionType.AndAssign or
            ExpressionType.AndAlso or
            ExpressionType.ArrayIndex or
            ExpressionType.Assign or
            ExpressionType.Coalesce or
            ExpressionType.Divide or
            ExpressionType.DivideAssign or
            ExpressionType.Equal or
            ExpressionType.ExclusiveOr or
            ExpressionType.ExclusiveOrAssign or
            ExpressionType.GreaterThan or
            ExpressionType.GreaterThanOrEqual or
            ExpressionType.LeftShift or
            ExpressionType.LeftShiftAssign or
            ExpressionType.LessThan or
            ExpressionType.LessThanOrEqual or
            ExpressionType.Modulo or
            ExpressionType.ModuloAssign or
            ExpressionType.Multiply or
            ExpressionType.MultiplyAssign or
            ExpressionType.MultiplyAssignChecked or
            ExpressionType.MultiplyChecked or
            ExpressionType.NotEqual or
            ExpressionType.Or or
            ExpressionType.OrAssign or
            ExpressionType.OrElse or
            ExpressionType.Power or
            ExpressionType.PowerAssign or
            ExpressionType.RightShift or
            ExpressionType.RightShiftAssign or
            ExpressionType.Subtract or
            ExpressionType.SubtractAssign or
            ExpressionType.SubtractAssignChecked or
            ExpressionType.SubtractChecked =>
                BinaryExpr((BinaryExpression)expr),

            ExpressionType.Block =>
                BlockExpr((BlockExpression)expr),
            ExpressionType.Call =>
                CallExpr((MethodCallExpression)expr),
            ExpressionType.Conditional =>
                ConditionalExpr((ConditionalExpression)expr),
            ExpressionType.Constant =>
                ConstantExpr((ConstantExpression)expr),
            ExpressionType.DebugInfo =>
                DebugInfoExpr((DebugInfoExpression)expr),
            ExpressionType.Default =>
                DefaultExpr((DefaultExpression)expr),
            ExpressionType.Dynamic =>
                DynamicExpr((DynamicExpression)expr),
            ExpressionType.Goto =>
                GotoExpr((GotoExpression)expr),
            ExpressionType.Index =>
                IndexExpr((IndexExpression)expr),
            ExpressionType.Invoke =>
                InvokeExpr((InvocationExpression)expr),
            ExpressionType.Label =>
                LabelExpr((LabelExpression)expr),
            ExpressionType.Lambda =>
                LambdaExpr((LambdaExpression)expr),
            ExpressionType.Loop =>
                LoopExpr((LoopExpression)expr),
            ExpressionType.ListInit =>
                ListInitExpr((ListInitExpression)expr),
            ExpressionType.MemberAccess =>
                MemberAccessExpr((MemberExpression)expr),
            ExpressionType.MemberInit =>
                MemberInitExpr((MemberInitExpression)expr),
            ExpressionType.New =>
                NewExpr((NewExpression)expr),

            ExpressionType.NewArrayBounds or
            ExpressionType.NewArrayInit =>
                NewArrayExpr((NewArrayExpression)expr),

            ExpressionType.Parameter =>
                ParameterExpr((ParameterExpression)expr),
            ExpressionType.RuntimeVariables =>
                RuntimeVariablesExpr((RuntimeVariablesExpression)expr),
            ExpressionType.Switch =>
                SwitchExpr((SwitchExpression)expr),
            ExpressionType.Try =>
                TryExpr((TryExpression)expr),

            ExpressionType.TypeEqual or
            ExpressionType.TypeIs =>
                TypeBinaryExpr((TypeBinaryExpression)expr),

            ExpressionType.Convert or
            ExpressionType.ConvertChecked or
            ExpressionType.Quote
            when Simplified =>
                ApplyImpl(((UnaryExpression)expr).Operand),

            ExpressionType.ArrayLength or
            ExpressionType.Convert or
            ExpressionType.ConvertChecked or
            ExpressionType.Decrement or
            ExpressionType.Increment or
            ExpressionType.IsFalse or
            ExpressionType.IsTrue or
            ExpressionType.Negate or
            ExpressionType.NegateChecked or
            ExpressionType.Not or
            ExpressionType.OnesComplement or
            ExpressionType.PostDecrementAssign or
            ExpressionType.PostIncrementAssign or
            ExpressionType.PreDecrementAssign or
            ExpressionType.PreIncrementAssign or
            ExpressionType.Quote or
            ExpressionType.Throw or
            ExpressionType.TypeAs or
            ExpressionType.UnaryPlus or
            ExpressionType.Unbox =>
                UnaryExpr((UnaryExpression)expr),

            // not sure what to do with this one

            ExpressionType.Extension => ApplyDefault(expr)
        };
    }
}
