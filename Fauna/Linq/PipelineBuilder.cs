using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Types;
using Fauna.Util;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using IE = Fauna.Linq.IntermediateExpr;

namespace Fauna.Linq;

internal class PipelineBuilder
{
    protected DataContext DataCtx { get; }
    protected MappingContext MappingCtx { get => DataCtx.MappingCtx; }
    protected Expression QueryExpr { get; }
    protected ParameterExpression CParam { get; }
    protected Dictionary<Type, Expression> CExprs { get; } = new();

    // outputs
    protected IDeserializer? Deserializer { get; set; }

    public PipelineBuilder(DataContext ctx, object[] closures, Expression expr)
    {
        DataCtx = ctx;
        QueryExpr = expr;
        CParam = Expression.Parameter(typeof(object[]));

        for (var i = 0; i < closures.Length; i++)
        {
            var ctype = closures[i].GetType();
            var cexpr = Expression.Convert(
                Expression.ArrayIndex(CParam, Expression.Constant(i)),
                ctype);

            CExprs[ctype] = cexpr;
        }
    }


    public Pipeline Build()
    {
        var ie = (new QuerySwitch(this)).Apply(QueryExpr);

        Debug.Assert(Deserializer is not null);

        var body = ie.Build();
        var func = Expression.Lambda<Func<object[], Query>>(body, CParam).Compile();

        return new Pipeline(func, Deserializer);
    }

    private class QuerySwitch : IEBaseSwitch
    {
        public QuerySwitch(PipelineBuilder builder) : base(builder) { }

        private IE SubQuery(Expression expr) => new SubquerySwitch(_builder).Apply(expr);

        private IE WhereCall(IE callee, Expression pred) =>
            IE.MethodCall(callee, "where", SubQuery(pred));

        private IDeserializer DocPageDeserializer(Type docType)
        {
            var pageType = typeof(Page<>).MakeGenericType(docType);
            return Serialization.Deserializer.Generate(_builder.MappingCtx, pageType);
        }

        // ExpressionSwitch

        protected override IE ConstantExpr(ConstantExpression expr)
        {
            if (expr.Value is DataContext.Collection col)
            {
                _builder.Deserializer = DocPageDeserializer(col.DocType);
            }
            else if (expr.Value is DataContext.Index idx)
            {
                _builder.Deserializer = DocPageDeserializer(idx.DocType);
            }

            return base.ConstantExpr(expr);
        }

        protected override IE CallExpr(MethodCallExpression expr)
        {
            IE ret;

            var (callee, args, ext) = GetCalleeAndArgs(expr);

            Debug.Assert(callee.Type.IsAssignableTo(typeof(IQueryable)));

            switch (expr.Method.Name)
            {
                case "Any" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "nonEmpty");
                    _builder.Deserializer = Serialization.Deserializer.Dynamic;
                    return ret;

                case "All" when args.Length == 1:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "every", SubQuery(args[0]));
                    _builder.Deserializer = Serialization.Deserializer.Dynamic;
                    return ret;

                case "Count" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "count");
                    _builder.Deserializer = Serialization.Deserializer.Dynamic;
                    return ret;

                case "Distinct" when args.Length == 0:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "distinct");
                    return ret;

                // TODO(matt) throw on empty
                case "First" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "first");
                    _builder.Deserializer = GetElemDeser(_builder.Deserializer);
                    return ret;

                case "FirstOrDefault" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "first");
                    _builder.Deserializer = GetElemDeser(_builder.Deserializer);
                    return ret;

                // TODO(matt) throw on empty
                case "Last" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "last");
                    _builder.Deserializer = GetElemDeser(_builder.Deserializer);
                    return ret;

                case "LastOrDefault" when args.Length <= 0:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "last");
                    _builder.Deserializer = GetElemDeser(_builder.Deserializer);
                    return ret;

                case "LongCount" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "count");
                    _builder.Deserializer = Serialization.Deserializer.Dynamic;
                    return ret;

                // TODO(matt) validate element type
                case "Max" when args.Length == 0:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "reduce", IE.Exp("(a, b) => if (a >= b) a else b"));
                    _builder.Deserializer = GetElemDeser(_builder.Deserializer);
                    return ret;

                // TODO(matt) validate element type
                case "Min" when args.Length == 0:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "reduce", IE.Exp("(a, b) => if (a <= b) a else b"));
                    _builder.Deserializer = GetElemDeser(_builder.Deserializer);
                    return ret;

                case "Reverse" when args.Length == 0:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "reverse");
                    return ret;

                // TODO(matt) reject variant which takes predicate with a second i32 parameter.
                case "Where" when args.Length == 1:
                    ret = Apply(callee);
                    ret = WhereCall(ret, args[0]);
                    return ret;

                default:
                    throw fail(expr);

            }
        }
    }

    private class SubquerySwitch : IEBaseSwitch
    {
        public SubquerySwitch(PipelineBuilder builder) : base(builder) { }

        protected override IE LambdaExpr(LambdaExpression expr)
        {
            var ps = expr.Parameters;
            var pinner = string.Join(", ", ps.Select(p => p.Name));
            var param = ps.Count() == 1 ? pinner : $"({pinner})";
            var arrow = IE.Exp($"{param} =>");

            return arrow.Concat(IE.Parens(Apply(expr.Body)));
        }

        protected override IE ParameterExpr(ParameterExpression expr) => IE.Exp(expr.Name!);

        protected override IE BinaryExpr(BinaryExpression expr)
        {
            var op = expr.NodeType switch
            {
                ExpressionType.Add => "+",
                ExpressionType.AddChecked => "+",
                ExpressionType.And => "&", // bitwise
                ExpressionType.AndAlso => "&&", // boolean
                // ExpressionType.ArrayIndex => ,
                ExpressionType.Coalesce => "??",
                ExpressionType.Divide => "/",
                ExpressionType.Equal => "==",
                ExpressionType.ExclusiveOr => "^",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LeftShift => "<<",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.Modulo => "%",
                ExpressionType.Multiply => "*",
                ExpressionType.MultiplyChecked => "*",
                ExpressionType.NotEqual => "!=",
                ExpressionType.Or => "|", // bitwise
                ExpressionType.OrElse => "||", // boolean
                ExpressionType.Power => "^",
                ExpressionType.RightShift => ">>",
                ExpressionType.Subtract => "-",
                ExpressionType.SubtractChecked => "-",
                _ => throw fail(expr)
            };

            var lhs = Apply(expr.Left);
            var rhs = Apply(expr.Right);

            return IE.Parens(IE.Op(lhs, op, rhs));
        }

        protected override IE CallExpr(MethodCallExpression expr)
        {
            var (callee, args, ext) = GetCalleeAndArgs(expr);
            return Lookup(expr.Method.Name, callee, args, expr) ?? throw fail(expr);
        }

        protected override IE MemberAccessExpr(MemberExpression expr)
        {
            if (expr.Expression is null)
            {
                var val = Expression.Lambda(expr).Compile().DynamicInvoke();
                return IE.Const(val);
            }
            else
            {
                var ty = expr.Expression.Type;
                IE? ret = null;

                ret = Lookup(expr.Member.Name, expr.Expression, expr);

                // no static lookup, do POCO field lookup
                if (ret is null)
                {
                    var info = _builder.MappingCtx.GetInfo(ty);
                    var field = info.Fields.FirstOrDefault(f => f.Property == expr.Member);

                    if (field is not null)
                    {
                        ret = IE.Field(Apply(expr.Expression), field.Name);
                    }
                }

                return ret ?? throw fail(expr);
            }
        }

        private IE? Lookup(string field, Expression callee, Expression orig) =>
            Lookup(field, callee, new Expression[] { }, orig);

        private IE? Lookup(string field, Expression callee, Expression[] args, Expression orig) =>
            callee.Type.Name switch
            {
                "string" => StringLookup(field, callee, args) ?? throw fail(orig),
                _ => null,
            };

        // method translation tables

        private IE? StringLookup(string method, Expression callee, Expression[] args) =>
            method switch
            {
                "Length" => IE.Field(Apply(callee), "length"),
                "EndsWith" => IE.MethodCall(Apply(callee), "endsWith", ApplyAll(args)),
                "StartsWith" => IE.MethodCall(Apply(callee), "startsWith", ApplyAll(args)),
                _ => null,
            };
    }

    private abstract class IEBaseSwitch : BaseSwitch<IE>
    {
        public IEBaseSwitch(PipelineBuilder builder) : base(builder) { }

        protected override IE ConstantExpr(ConstantExpression expr)
        {
            if (expr.Value is DataContext.Collection col)
            {
                return new IE.Expr($"{col.Name}.all()");
            }
            else if (expr.Value is DataContext.Index idx)
            {
                return BuildIndexCall(idx);
            }
            else if (_builder.CExprs.TryGetValue(expr.Type, out var cexpr))
            {
                return new IE.Closure(cexpr);
            }
            else
            {
                return new IE.Constant(expr.Value);
            }
        }

        protected IE BuildIndexCall(DataContext.Index idx) =>
            IE.MethodCall(IE.Exp(idx.Collection.Name), idx.Name, idx.Args.Select(a => IE.Const(a)));
    }

    private abstract class BaseSwitch<T> : DefaultExpressionSwitch<T>
    {
        protected readonly PipelineBuilder _builder;

        public BaseSwitch(PipelineBuilder builder)
        {
            _builder = builder;
        }

        protected override T ApplyDefault(Expression? expr) => throw fail(expr);
    }

    // Helpers

    // TODO(matt) use an API-specific exception in-line with what other LINQ
    // libraries do.
    private static Exception fail(Expression? expr) =>
        new NotSupportedException($"Unsupported {expr?.NodeType} expression: {expr}");

    private static IDeserializer GetElemDeser(IDeserializer? deser)
    {
        if (deser is PageDeserializer pd)
        {
            return pd.Elem;
        }
        throw new ArgumentException($"{nameof(deser)} is not a PageDeserializer ({deser})");
    }

    private static (Expression, Expression[], bool) GetCalleeAndArgs(MethodCallExpression expr) =>
         expr.Object switch
         {
             null => (expr.Arguments.First(), expr.Arguments.Skip(1).ToArray(), true),
             var c => (c, expr.Arguments.ToArray(), false),
         };
}
