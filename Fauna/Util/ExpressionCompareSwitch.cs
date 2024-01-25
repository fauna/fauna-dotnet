using System.Linq.Expressions;

namespace Fauna.Util;

internal class ExpressionCompareSwitch : ExpressionSwitch<Boolean>
{
    protected Expression _lhs;

    public ExpressionCompareSwitch(Expression lhs)
    {
        _lhs = lhs;
    }

    protected override bool Simplified { get => false; }

    protected bool Eq(Expression? lhs, Expression? rhs)
    {
        if (ReferenceEquals(lhs, rhs))
            return true;
        else if (lhs is null || rhs is null)
            return false;

        _lhs = lhs;
        return Apply(rhs);
    }

    protected bool AllEq(IEnumerable<Expression> lhs, IEnumerable<Expression> rhs) =>
        AllEq(lhs, rhs, (l, r) => Eq(l, r));

    protected bool AllEq<E>(IEnumerable<E> lhs, IEnumerable<E> rhs, Func<E, E, Boolean> cond)
    {
        var lhsEnum = lhs.GetEnumerator();
        var rhsEnum = rhs.GetEnumerator();
        var lmove = lhsEnum.MoveNext();
        var rmove = rhsEnum.MoveNext();

        while (lmove && rmove)
        {
            if (!cond(lhsEnum.Current, rhsEnum.Current))
            {
                return false;
            }

            // advance each enumerator separate from the conditional so that
            // lmove == rmove below is true if lengths are equal.
            lmove = lhsEnum.MoveNext();
            rmove = rhsEnum.MoveNext();
        }

        return lmove == rmove;
    }

    protected override bool NullExpr() => _lhs == null;

    protected override bool BinaryExpr(BinaryExpression rhs)
    {
        var lhs = _lhs as BinaryExpression;
        if (lhs is null) return false;

        return lhs.Method == rhs.Method &&
            Eq(lhs.Left, rhs.Left) &&
            Eq(lhs.Right, rhs.Right);
    }

    protected override bool BlockExpr(BlockExpression rhs)
    {
        var lhs = _lhs as BlockExpression;
        if (lhs is null) return false;

        return AllEq(lhs.Expressions, rhs.Expressions);
    }

    protected override bool ConditionalExpr(ConditionalExpression rhs)
    {
        var lhs = _lhs as ConditionalExpression;
        if (lhs is null) return false;

        return Eq(lhs.Test, rhs.Test) &&
            Eq(lhs.IfTrue, rhs.IfTrue) &&
            Eq(lhs.IfFalse, rhs.IfFalse);
    }

    protected override bool CallExpr(MethodCallExpression rhs)
    {
        var lhs = _lhs as MethodCallExpression;
        if (lhs is null) return false;

        return lhs.Method == rhs.Method &&
            Eq(lhs.Object, rhs.Object) &&
            AllEq(lhs.Arguments, rhs.Arguments);
    }

    protected override bool ConstantExpr(ConstantExpression rhs)
    {
        var lhs = _lhs as ConstantExpression;
        if (lhs is null) return false;

        return lhs.Value == rhs.Value;
    }

    protected override bool DebugInfoExpr(DebugInfoExpression rhs)
    {
        var lhs = _lhs as DebugInfoExpression;
        if (lhs is null) return false;

        return lhs.Document == rhs.Document &&
            lhs.EndColumn == rhs.EndColumn &&
            lhs.EndLine == rhs.EndLine &&
            lhs.IsClear == rhs.IsClear &&
            lhs.StartColumn == rhs.StartColumn &&
            lhs.StartLine == rhs.StartLine;
    }

    protected override bool DefaultExpr(DefaultExpression rhs)
    {
        var lhs = _lhs as DefaultExpression;
        if (lhs is null) return false;

        return lhs.Type == rhs.Type;
    }

    // pretty sure this one is not allowed in LINQ exprs
    protected override bool DynamicExpr(DynamicExpression rhs)
    {
        var lhs = _lhs as DynamicExpression;
        if (lhs is null) return false;

        // FIXME(matt) Binder equality is reference-based. not sure if this is right.
        return lhs.Binder == rhs.Binder &&
            lhs.DelegateType == rhs.DelegateType &&
            AllEq(lhs.Arguments, rhs.Arguments);
    }

    protected override bool GotoExpr(GotoExpression rhs)
    {
        var lhs = _lhs as GotoExpression;
        if (lhs is null) return false;

        return lhs.Target.Name == rhs.Target.Name &&
            lhs.Value == rhs.Value;
    }

    protected override bool IndexExpr(IndexExpression rhs)
    {
        var lhs = _lhs as IndexExpression;
        if (lhs is null) return false;

        return (lhs.Indexer == rhs.Indexer) &&
            Eq(lhs.Object, rhs.Object) &&
            AllEq(lhs.Arguments, rhs.Arguments);
    }

    protected override bool InvokeExpr(InvocationExpression rhs)
    {
        var lhs = _lhs as InvocationExpression;
        if (lhs is null) return false;

        return Eq(lhs.Expression, rhs.Expression) &&
            AllEq(lhs.Arguments, rhs.Arguments);
    }

    protected override bool LabelExpr(LabelExpression rhs)
    {
        var lhs = _lhs as LabelExpression;
        if (lhs is null) return false;

        return lhs.Target.Name == rhs.Target.Name &&
            lhs.Target.Type == rhs.Target.Type &&
            Eq(lhs.DefaultValue, rhs.DefaultValue);
    }

    protected override bool LambdaExpr(LambdaExpression rhs)
    {
        var lhs = _lhs as LambdaExpression;
        if (lhs is null) return false;

        return lhs.Name == rhs.Name &&
            AllEq(lhs.Parameters, rhs.Parameters) &&
            Eq(lhs.Body, rhs.Body);
    }

    protected override bool ListInitExpr(ListInitExpression rhs)
    {
        var lhs = _lhs as ListInitExpression;
        if (lhs is null) return false;

        return Eq(lhs.NewExpression, rhs.NewExpression) &&
            AllEq(lhs.Initializers, rhs.Initializers, (li, ri) =>
                  li.AddMethod == ri.AddMethod &&
                  AllEq(li.Arguments, ri.Arguments));
    }

    protected override bool LoopExpr(LoopExpression rhs)
    {
        var lhs = _lhs as LoopExpression;
        if (lhs is null) return false;

        bool LabelEq(LabelTarget? l1, LabelTarget? l2)
        {
            if (ReferenceEquals(l1, l2)) return true;
            if (l1 == null || l2 == null) return false;

            return l1.Name == l2.Name;
        }

        return LabelEq(lhs.BreakLabel, rhs.BreakLabel) &&
            LabelEq(lhs.ContinueLabel, rhs.ContinueLabel) &&
            Eq(lhs.Body, rhs.Body);
    }

    protected override bool MemberAccessExpr(MemberExpression rhs)
    {
        var lhs = _lhs as MemberExpression;
        if (lhs is null) return false;

        return lhs.Member == rhs.Member &&
            Eq(lhs.Expression, rhs.Expression);
    }

    protected override bool MemberInitExpr(MemberInitExpression rhs)
    {
        var lhs = _lhs as MemberInitExpression;
        if (lhs is null) return false;

        return Eq(lhs.NewExpression, rhs.NewExpression) &&
            AllEq(lhs.Bindings, rhs.Bindings, (lm, rm) =>
                  lm.Member == rm.Member &&
                  lm.BindingType == rm.BindingType);
    }

    protected override bool NewArrayExpr(NewArrayExpression rhs)
    {
        var lhs = _lhs as NewArrayExpression;
        if (lhs is null) return false;

        return AllEq(lhs.Expressions, rhs.Expressions);
    }

    protected override bool NewExpr(NewExpression rhs)
    {
        var lhs = _lhs as NewExpression;
        if (lhs is null) return false;

        return lhs.Constructor == rhs.Constructor &&
            AllEq(lhs.Arguments, rhs.Arguments);
    }

    // FIXME(matt) in usage two parameters are equal based on reference equality...
    protected override bool ParameterExpr(ParameterExpression rhs)
    {
        var lhs = _lhs as ParameterExpression;
        if (lhs is null) return false;

        return lhs.Name == rhs.Name &&
            lhs.IsByRef == rhs.IsByRef;
    }

    protected override bool RuntimeVariablesExpr(RuntimeVariablesExpression rhs)
    {
        var lhs = _lhs as RuntimeVariablesExpression;
        if (lhs is null) return false;

        return AllEq(lhs.Variables, rhs.Variables) &&
            lhs.Type == rhs.Type;
    }

    protected override bool SwitchExpr(SwitchExpression rhs)
    {
        var lhs = _lhs as SwitchExpression;
        if (lhs is null) return false;

        return Eq(lhs.SwitchValue, rhs.SwitchValue) &&
            lhs.Comparison == rhs.Comparison &&
            AllEq(lhs.Cases, rhs.Cases, (lc, rc) =>
                  AllEq(lc.TestValues, rc.TestValues) &&
                  Eq(lc.Body, rc.Body)) &&
            Eq(lhs.DefaultBody, rhs.DefaultBody);
    }

    protected override bool TryExpr(TryExpression rhs)
    {
        var lhs = _lhs as TryExpression;
        if (lhs is null) return false;

        return Eq(lhs.Body, rhs.Body) &&
            Eq(lhs.Fault, rhs.Fault) &&
            Eq(lhs.Finally, rhs.Finally) &&
            AllEq(lhs.Handlers, rhs.Handlers, (lc, rc) =>
                  lc.Test == rc.Test &&
                  Eq(lc.Variable, rc.Variable) &&
                  Eq(lc.Filter, rc.Filter) &&
                  Eq(lc.Body, rc.Body));
    }

    protected override bool TypeBinaryExpr(TypeBinaryExpression rhs)
    {
        var lhs = _lhs as TypeBinaryExpression;
        if (lhs is null) return false;

        return lhs.TypeOperand == rhs.TypeOperand &&
            Eq(lhs.Expression, rhs.Expression);
    }

    protected override bool UnaryExpr(UnaryExpression rhs)
    {
        var lhs = _lhs as UnaryExpression;
        if (lhs is null) return false;

        return lhs.Method == rhs.Method &&
            Eq(lhs.Operand, rhs.Operand);
    }

    protected override bool UnknownExpr(Expression rhs)
    {
        // Not really anything else to do...
        return ReferenceEquals(_lhs, rhs);
    }
}
