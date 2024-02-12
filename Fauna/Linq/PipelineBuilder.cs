using Fauna.Mapping;
using Fauna.Serialization;
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
    protected LookupTable Lookup { get; }
    protected Expression QueryExpr { get; }
    protected ParameterExpression CParam { get; }
    protected Dictionary<Type, Expression> CExprs { get; } = new();

    // outputs

    protected PipelineMode Mode { get; set; }
    protected Type? ElemType { get; set; }
    protected IDeserializer? ElemDeserializer { get; set; }
    protected LambdaExpression? ProjectExpr { get; set; }

    public PipelineBuilder(DataContext ctx, object[] closures, Expression expr)
    {
        DataCtx = ctx;
        Lookup = new LookupTable(ctx.MappingCtx);
        QueryExpr = expr;
        CParam = Expression.Parameter(typeof(object[]));

        for (var i = 0; i < closures.Length; i++)
        {
            var ctype = closures[i].GetType();
            CExprs[ctype] = Expression.Convert(
                Expression.ArrayIndex(CParam, Expression.Constant(i)),
                ctype);
        }
    }

    public Pipeline Build()
    {
        var ie = (new QuerySwitch(this)).Apply(QueryExpr);

        Debug.Assert(ElemDeserializer is not null);
        Debug.Assert(ElemType is not null);

        var query = Expression.Lambda<Func<Query>>(ie.Build()).Compile().Invoke();
        var deser = ElemDeserializer;
        var pfunc = ProjectExpr is null ? null : ProjectExpr.Compile();

        return new Pipeline(query, deser, pfunc, Mode);
    }

    // QuerySwitch handles the top-level method chain, but not lambdas in predicates/projections.
    private class QuerySwitch : BuilderSwitch<IE>
    {
        public QuerySwitch(PipelineBuilder builder) : base(builder) { }

        private IDeserializer TypeDeserializer(Type ty) =>
            Serialization.Deserializer.Generate(_builder.MappingCtx, ty);

        private IE SubQuery(Expression expr) =>
            new SubquerySwitch(_builder).Apply(expr);

        private void SetElemType(Type ty, IDeserializer? deser = null)
        {
            _builder.ProjectExpr = null;
            _builder.ElemType = ty;
            _builder.ElemDeserializer = deser ?? TypeDeserializer(ty);
        }

        private IE WhereCall(IE callee, Expression pred)
        {
            // TODO(matt) allow Where on later stages by moving to loaded set
            // transforms.
            Debug.Assert(_builder.Mode <= PipelineMode.Query);
            return IE.MethodCall(callee, "where", SubQuery(pred));
        }

        private IE SelectCall(IE callee, Expression proj)
        {
            // TODO(matt) allow Select on later stages by moving to loaded set
            // transforms.
            Debug.Assert(_builder.Mode <= PipelineMode.Project);

            var lambda = Expressions.UnwrapLambda(proj);
            Debug.Assert(lambda is not null, $"lambda is {proj.NodeType}");
            Debug.Assert(lambda.Parameters.Count() == 1);

            // there is already a projection wired up, so tack on to its mapping lambda
            if (_builder.Mode == PipelineMode.Project)
            {
                Debug.Assert(_builder.ProjectExpr is not null);
                var prev = _builder.ProjectExpr;
                var pbody = Expression.Invoke(lambda, new Expression[] { prev.Body });
                var plambda = Expression.Lambda(pbody, prev.Parameters);
                _builder.ProjectExpr = plambda;

                return callee;
            }

            Debug.Assert(_builder.Mode == PipelineMode.Query);

            var lparam = lambda.Parameters.First()!;
            var analysis = new ProjectionAnalysisVisitor(_builder.MappingCtx, lparam);
            analysis.Visit(lambda.Body);

            // select is a simple field access which we can translate directly to FQL.
            // TODO(matt) translate more cases to pure FQL
            if (lambda.Body is MemberExpression mexpr && mexpr.Expression == lparam)
            {
                Debug.Assert(!analysis.Escapes);
                var info = _builder.MappingCtx.GetInfo(lparam.Type);
                var access = analysis.Accesses.First();
                var field = _builder.Lookup.FieldLookup(access, lparam);
                Debug.Assert(field is not null);

                SetElemType(field.Type, field.Deserializer);

                var pquery = IE.Exp("x => ").Concat(IE.Field(IE.Exp("x"), field.Name));
                return IE.MethodCall(callee, "map", pquery);
            }

            // handle the case where some value mapping must occur locally

            _builder.Mode = PipelineMode.Project;

            if (analysis.Escapes)
            {
                _builder.ProjectExpr = lambda;
                return callee;
            }
            else
            {
                var accesses = analysis.Accesses.OrderBy(f => f.Name).ToArray();
                var fields = accesses.Select(a => _builder.Lookup.FieldLookup(a, lparam)!);

                // projected field deserializer
                var deserializer = new ProjectionDeserializer(fields.Select(f => f.Deserializer));
                SetElemType(typeof(object?[]), deserializer);

                // build mapping lambda expression
                var pparam = Expression.Parameter(typeof(object?[]), "x");
                var rewriter = new ProjectionRewriteVisitor(lparam, accesses, pparam);
                var pbody = rewriter.Visit(lambda.Body);
                var plambda = Expression.Lambda(pbody, pparam);
                _builder.ProjectExpr = plambda;

                // projection query fragment
                var accs = fields.Select(f => IE.Field(IE.Exp("x"), f.Name));
                var pquery = IE.Exp("x => ").Concat(IE.Array(accs));
                return IE.MethodCall(callee, "map", pquery);
            }
        }

        // ExpressionSwitch

        protected override IE ConstantExpr(ConstantExpression expr)
        {
            Debug.Assert(_builder.Mode == PipelineMode.Query);

            if (expr.Value is DataContext.Collection col)
            {
                SetElemType(col.DocType);
                return CollectionAll(col);
            }
            else if (expr.Value is DataContext.Index idx)
            {
                SetElemType(idx.DocType);
                return CollectionIndex(idx);
            }
            else
            {
                // Queries must start with an expected query source.
                throw fail(expr);
            }
        }

        protected override IE CallExpr(MethodCallExpression expr)
        {
            IE ret;

            var (callee, args, ext) = GetCalleeAndArgs(expr);

            Debug.Assert(callee.Type.IsAssignableTo(typeof(IQuerySource)));
            Debug.Assert(_builder.Mode != PipelineMode.Scalar);

            switch (expr.Method.Name)
            {
                case "All" when args.Length == 1:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "every", SubQuery(args[0]));
                    SetElemType(typeof(bool));
                    _builder.Mode = PipelineMode.Scalar;
                    return ret;

                case "Any" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "nonEmpty");
                    SetElemType(typeof(bool));
                    _builder.Mode = PipelineMode.Scalar;
                    return ret;

                case "Count" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "count");
                    SetElemType(typeof(int));
                    _builder.Mode = PipelineMode.Scalar;
                    return ret;

                case "Distinct" when args.Length == 0 && _builder.Mode == PipelineMode.Query:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "distinct");
                    return ret;

                // TODO(matt) throw on empty
                case "First" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "first");
                    _builder.Mode = PipelineMode.Scalar;
                    return ret;

                case "FirstOrDefault" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "first");
                    _builder.Mode = PipelineMode.Scalar;
                    return ret;

                // FIXME(matt) have throw on empty (tack on to ProjectExpr)
                case "Last" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "last");
                    _builder.Mode = PipelineMode.Scalar;
                    return ret;

                // FIXME(matt) have return default on empty (tack on to ProjectExpr?)
                case "LastOrDefault" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "last");
                    _builder.Mode = PipelineMode.Scalar;
                    return ret;

                case "LongCount" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "count");
                    SetElemType(typeof(long));
                    _builder.Mode = PipelineMode.Scalar;
                    return ret;

                case "Max" when args.Length == 0 && _builder.Mode == PipelineMode.Query:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "reduce", IE.Exp("(a, b) => if (a >= b) a else b"));
                    _builder.Mode = PipelineMode.Scalar;
                    return ret;

                case "Min" when args.Length == 0 && _builder.Mode == PipelineMode.Query:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "reduce", IE.Exp("(a, b) => if (a <= b) a else b"));
                    _builder.Mode = PipelineMode.Scalar;
                    return ret;

                case "Order" when args.Length == 0 && _builder.Mode == PipelineMode.Query:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "order");
                    return ret;

                case "OrderBy" when args.Length == 1 && _builder.Mode == PipelineMode.Query:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "order", SubQuery(args[0]));
                    return ret;

                case "OrderDescending" when args.Length == 0 && _builder.Mode == PipelineMode.Query:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "order", IE.Exp("desc(x => x)"));
                    return ret;

                case "OrderByDescending" when args.Length == 1 && _builder.Mode == PipelineMode.Query:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "order", IE.FnCall("desc", SubQuery(args[0])));
                    return ret;

                case "Reverse" when args.Length == 0:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "reverse");
                    return ret;

                case "Select" when args.Length == 1:
                    ret = Apply(callee);
                    ret = SelectCall(ret, args[0]);
                    return ret;

                case "Skip" when args.Length == 1 && _builder.Mode == PipelineMode.Query:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "drop", SubQuery(args[0]));
                    return ret;

                case "Sum" when args.Length == 0 && _builder.Mode == PipelineMode.Query:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "reduce", IE.Exp("(a, b) => a + b"));
                    SetElemType(expr.Type);
                    _builder.Mode = PipelineMode.Scalar;
                    return ret;

                case "Take" when args.Length == 1 && _builder.Mode == PipelineMode.Query:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "take", SubQuery(args[0]));
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

    private class SubquerySwitch : BuilderSwitch<IE>
    {
        public SubquerySwitch(PipelineBuilder builder) : base(builder) { }

        protected override IE ConstantExpr(ConstantExpression expr)
        {
            if (expr.Value is DataContext.Collection col)
            {
                return CollectionAll(col);
            }
            else if (expr.Value is DataContext.Index idx)
            {
                return CollectionIndex(idx);
            }
            else
            {
                return new IE.Constant(expr.Value);
            }
        }

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
                ExpressionType.Power => "**",
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
            var name = _builder.Lookup.MethodLookup(expr.Method, callee)?.Name;
            if (name is null) throw fail(expr);
            return IE.MethodCall(Apply(callee), name, ApplyAll(args));
        }

        protected override IE MemberAccessExpr(MemberExpression expr)
        {
            var callee = expr.Expression;
            if (callee is null)
            {
                var val = Expression.Lambda(expr).Compile().DynamicInvoke();
                return IE.Const(val);
            }
            else if (callee.Type.IsClosureType())
            {
                return Apply(callee).Access(expr.Member.Name);
            }
            else
            {
                var name = expr.Member is PropertyInfo prop ?
                    _builder.Lookup.FieldLookup(prop, callee)?.Name :
                    null;

                if (name is null) throw fail(expr);
                return IE.Field(Apply(callee), name);
            }
        }
    }

    private abstract class BuilderSwitch<T> : DefaultExpressionSwitch<T>
    {
        protected readonly PipelineBuilder _builder;

        public BuilderSwitch(PipelineBuilder builder)
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

    private static IE CollectionAll(DataContext.Collection col) =>
        IE.MethodCall(IE.Exp(col.Name), "all");

    private static IE CollectionIndex(DataContext.Index idx) =>
        IE.MethodCall(IE.Exp(idx.Collection.Name), idx.Name, idx.Args.Select(a => IE.Const(a)));

    private static (Expression, Expression[], bool) GetCalleeAndArgs(MethodCallExpression expr) =>
         expr.Object switch
         {
             null => (expr.Arguments.First(), expr.Arguments.Skip(1).ToArray(), true),
             var c => (c, expr.Arguments.ToArray(), false),
         };
}
