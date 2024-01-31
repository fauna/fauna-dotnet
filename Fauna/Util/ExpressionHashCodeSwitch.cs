using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Fauna.Util;

internal class ExpressionHashCodeSwitch : ExpressionSwitch<int>
{
    protected int Base(Expression e) => 7 * e.NodeType.GetHashCode();

    protected override int NullExpr() => RuntimeHelpers.GetHashCode(null);

    protected override int BinaryExpr(BinaryExpression e) =>
        Base(e) * RuntimeHelpers.GetHashCode(e.Method) * Apply(e.Left) * Apply(e.Right);

    protected override int BlockExpr(BlockExpression e) =>
        Base(e) * e.Expressions.Aggregate(1, (h, e) => h * Apply(e));

    protected override int ConditionalExpr(ConditionalExpression e) =>
        Base(e) * Apply(e.Test) * Apply(e.IfTrue) * Apply(e.IfFalse);

    protected override int CallExpr(MethodCallExpression e) =>
        Base(e) * e.Method.GetHashCode() * Apply(e.Object) *
        e.Arguments.Aggregate(1, (h, e) => h * Apply(e));

    protected override int ConstantExpr(ConstantExpression e) =>
        Base(e) * RuntimeHelpers.GetHashCode(e.Value);

    protected override int DebugInfoExpr(DebugInfoExpression e) =>
        Base(e) * e.Document.GetHashCode() * e.EndColumn.GetHashCode() * e.EndLine.GetHashCode() *
        e.IsClear.GetHashCode() * e.StartColumn.GetHashCode() * e.StartLine.GetHashCode();

    protected override int DefaultExpr(DefaultExpression e) =>
        Base(e) * e.Type.GetHashCode();

    protected override int DynamicExpr(DynamicExpression e) =>
        Base(e) * e.Binder.GetHashCode() * e.DelegateType.GetHashCode() *
        e.Arguments.Aggregate(1, (h, e) => h * Apply(e));

    protected override int GotoExpr(GotoExpression e) =>
        Base(e) * RuntimeHelpers.GetHashCode(e.Target.Name) * Apply(e.Value);

    protected override int IndexExpr(IndexExpression e) =>
        Base(e) * RuntimeHelpers.GetHashCode(e.Indexer) * Apply(e.Object) *
        e.Arguments.Aggregate(1, (h, e) => h * Apply(e));

    protected override int InvokeExpr(InvocationExpression e) =>
        Base(e) * Apply(e.Expression) * e.Arguments.Aggregate(1, (h, e) => h * Apply(e));

    protected override int LabelExpr(LabelExpression e) =>
        Base(e) * RuntimeHelpers.GetHashCode(e.Target.Name) * e.Target.Type.GetHashCode() *
        Apply(e.DefaultValue);

    protected override int LambdaExpr(LambdaExpression e) =>
        Base(e) * RuntimeHelpers.GetHashCode(e.Name) * Apply(e.Body) *
        e.Parameters.Aggregate(1, (h, e) => h * Apply(e));

    protected override int ListInitExpr(ListInitExpression e) =>
        Base(e) * Apply(e.NewExpression) *
        e.Initializers.Aggregate(1, (h, i) =>
                                 h * i.AddMethod.GetHashCode() *
                                 i.Arguments.Aggregate(1, (h, e) => h * Apply(e)));

    protected override int LoopExpr(LoopExpression e) =>
        Base(e) * RuntimeHelpers.GetHashCode(e.BreakLabel?.Name) *
        RuntimeHelpers.GetHashCode(e.ContinueLabel?.Name) * Apply(e.Body);

    protected override int MemberAccessExpr(MemberExpression e) =>
        Base(e) * e.Member.GetHashCode() * Apply(e.Expression);

    protected override int MemberInitExpr(MemberInitExpression e) =>
        Base(e) * Apply(e.NewExpression) *
        e.Bindings.Aggregate(1, (h, e) =>
                             h * e.Member.GetHashCode() *
                             e.BindingType.GetHashCode());

    protected override int NewArrayExpr(NewArrayExpression e) =>
        Base(e) * e.Expressions.Aggregate(1, (h, e) => h * Apply(e));

    protected override int NewExpr(NewExpression e) =>
        Base(e) * RuntimeHelpers.GetHashCode(e.Constructor) *
        e.Arguments.Aggregate(1, (h, e) => h * Apply(e));

    // FIXME(matt) in usage two parameters are equal based on reference equality...
    protected override int ParameterExpr(ParameterExpression e) =>
        Base(e) * RuntimeHelpers.GetHashCode(e.Name) * e.IsByRef.GetHashCode();

    protected override int RuntimeVariablesExpr(RuntimeVariablesExpression e) =>
        Base(e) * e.Variables.Aggregate(1, (h, e) => h * Apply(e));

    protected override int SwitchExpr(SwitchExpression e) =>
        Base(e) * Apply(e.SwitchValue) * RuntimeHelpers.GetHashCode(e.Comparison) *
        e.Cases.Aggregate(1, (h, e) =>
                          h * e.TestValues.Aggregate(1, (h, e) => h * Apply(e)) *
                          Apply(e.Body)) *
        Apply(e.DefaultBody);

    protected override int TryExpr(TryExpression e) =>
        Base(e) * Apply(e.Body) * Apply(e.Fault) * Apply(e.Finally) *
        e.Handlers.Aggregate(1, (h, e) =>
                             h * e.Test.GetHashCode() *
                             Apply(e.Variable) *
                             Apply(e.Filter) *
                             Apply(e.Body));

    protected override int TypeBinaryExpr(TypeBinaryExpression e) =>
        Base(e) * e.TypeOperand.GetHashCode() * Apply(e.Expression);

    protected override int UnaryExpr(UnaryExpression e) =>
        Base(e) * RuntimeHelpers.GetHashCode(e.Method) * Apply(e.Operand);

    protected override int UnknownExpr(Expression e) =>
        Base(e) * RuntimeHelpers.GetHashCode(e);
}
