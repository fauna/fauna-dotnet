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
    internal enum BuildStage
    {
        Query, // "pure" query state. no local processing required (except deserialization)
        Project, // elements have been projected
        SetLoad, // post-processing on loaded set required
        Scalar, // final, non-enum result: no more transformations allowed
    }

    protected DataContext DataCtx { get; }
    protected MappingContext MappingCtx { get => DataCtx.MappingCtx; }
    protected LookupTable Lookup { get; }
    protected Expression QueryExpr { get; }
    protected ParameterExpression CParam { get; }
    protected Dictionary<Type, Expression> CExprs { get; } = new();

    // outputs

    protected BuildStage Stage { get; set; }
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
            var cexpr = Expression.Convert(
                Expression.ArrayIndex(CParam, Expression.Constant(i)),
                ctype);

            CExprs[ctype] = cexpr;
        }
    }

    public Pipeline Build()
    {
        var ie = (new QuerySwitch(this)).Apply(QueryExpr);

        Debug.Assert(ElemDeserializer is not null);
        Debug.Assert(ElemType is not null);

        var qfunc = Expression.Lambda<Func<object[], Query>>(ie.Build(), CParam).Compile();
        var deser = Stage == BuildStage.Scalar ?
            ElemDeserializer :
            PageDeserializer.Create(ElemType, ElemDeserializer);
        // var pfunc = ProjectExpr is null ? null : Expression.Lambda(body, CParam).Compile();

        return new Pipeline(qfunc, deser);
    }

    // QuerySwitch handles the top-level method chain, but not lambdas in predicates/projections.
    private class QuerySwitch : IEBaseSwitch
    {
        public QuerySwitch(PipelineBuilder builder) : base(builder) { }

        private IDeserializer TypeDeserializer(Type ty) =>
            Serialization.Deserializer.Generate(_builder.MappingCtx, ty);

        private IE SubQuery(Expression expr) =>
            new SubquerySwitch(_builder).Apply(expr);

        private Expression SubstClosures(Expression expr) =>
            Expressions.SubstituteByType(expr, _builder.CExprs);

        private void ResetProjection()
        {
            // reset the first two, so that state blow up if used incorrectly.
            _builder.ElemType = null;
            _builder.ElemDeserializer = null;
            _builder.ProjectExpr = null;
        }

        private void SetElemType(Type ty, IDeserializer? deser = null)
        {
            _builder.ElemType = ty;
            _builder.ElemDeserializer = deser ?? TypeDeserializer(ty);
        }

        private IE WhereCall(IE callee, Expression pred)
        {
            // TODO(matt) allow Where on later stages by moving to loaded set
            // transforms.
            Debug.Assert(_builder.Stage <= BuildStage.Query);
            return IE.MethodCall(callee, "where", SubQuery(pred));
        }

        private IE SelectCall(IE callee, Expression proj)
        {
            // TODO(matt) allow Select on later stages by moving to loaded set
            // transforms.
            Debug.Assert(_builder.Stage <= BuildStage.Project);

            var lambda = Expressions.UnwrapLambda(proj);
            Debug.Assert(lambda is not null, $"lambda is {proj.NodeType}");
            Debug.Assert(lambda.Parameters.Count() == 1);

            // need to rewrite closures so the lambda is reusable across invocations
            lambda = (LambdaExpression)SubstClosures(lambda);

            Debug.Assert(_builder.Stage == BuildStage.Query);

            var lparam = lambda.Parameters.First()!;
            var analysis = new ProjectAnalysisVisitor(_builder.MappingCtx, lparam);
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
            throw fail(proj);
        }

        // ExpressionSwitch

        protected override IE ConstantExpr(ConstantExpression expr)
        {
            Debug.Assert(_builder.Stage == BuildStage.Query);

            if (expr.Value is DataContext.Collection col)
            {
                SetElemType(col.DocType);
            }
            else if (expr.Value is DataContext.Index idx)
            {
                SetElemType(idx.DocType);
            }
            else
            {
                // Queries must start with an expected query source.
                throw fail(expr);
            }

            return base.ConstantExpr(expr);
        }

        protected override IE CallExpr(MethodCallExpression expr)
        {
            IE ret;

            var (callee, args, ext) = GetCalleeAndArgs(expr);

            Debug.Assert(callee.Type.IsAssignableTo(typeof(IQueryable)));
            Debug.Assert(_builder.Stage != BuildStage.Scalar);

            switch (expr.Method.Name)
            {
                case "Any" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "nonEmpty");
                    ResetProjection();
                    SetElemType(typeof(bool));
                    _builder.Stage = BuildStage.Scalar;
                    return ret;

                case "All" when args.Length == 1:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "every", SubQuery(args[0]));
                    ResetProjection();
                    SetElemType(typeof(bool));
                    _builder.Stage = BuildStage.Scalar;
                    return ret;

                case "Count" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "count");
                    ResetProjection();
                    SetElemType(typeof(int));
                    _builder.Stage = BuildStage.Scalar;
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
                    _builder.Stage = BuildStage.Scalar;
                    return ret;

                case "FirstOrDefault" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "first");
                    _builder.Stage = BuildStage.Scalar;
                    return ret;

                // FIXME(matt) have throw on empty (tack on to ProjectExpr)
                case "Last" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "last");
                    _builder.Stage = BuildStage.Scalar;
                    return ret;

                // FIXME(matt) have return default on empty (tack on to ProjectExpr?)
                case "LastOrDefault" when args.Length <= 0:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "last");
                    _builder.Stage = BuildStage.Scalar;
                    return ret;

                case "LongCount" when args.Length <= 1:
                    ret = Apply(callee);
                    if (args.Length == 1) ret = WhereCall(ret, args[0]);
                    ret = IE.MethodCall(ret, "count");
                    ResetProjection();
                    SetElemType(typeof(long));
                    _builder.Stage = BuildStage.Scalar;
                    return ret;

                case "Max" when args.Length == 0 && _builder.Stage == BuildStage.Query:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "reduce", IE.Exp("(a, b) => if (a >= b) a else b"));
                    _builder.Stage = BuildStage.Scalar;
                    return ret;

                case "Min" when args.Length == 0 && _builder.Stage == BuildStage.Query:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "reduce", IE.Exp("(a, b) => if (a <= b) a else b"));
                    _builder.Stage = BuildStage.Scalar;
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

                case "Select" when args.Length == 1:
                    ret = Apply(callee);
                    ret = SelectCall(ret, args[0]);
                    return ret;

                case "Sum" when args.Length == 0 && _builder.Stage == BuildStage.Query:
                    ret = Apply(callee);
                    ret = IE.MethodCall(ret, "reduce", IE.Exp("(a, b) => a + b"));
                    ResetProjection();
                    SetElemType(expr.Type);
                    _builder.Stage = BuildStage.Scalar;
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
            var name = _builder.Lookup.MethodLookup(expr.Method, callee)?.Name;
            return BuildAccess(name, callee, args) ?? throw fail(expr);
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
                var name = expr.Member is PropertyInfo prop ?
                    _builder.Lookup.FieldLookup(prop, expr.Expression)?.Name :
                    null;

                return BuildAccess(name, expr.Expression, null) ?? throw fail(expr);
            }
        }

        private IE? BuildAccess(string? name, Expression callee, Expression[]? args)
        {
            if (name is null) return null;
            if (args is null) return IE.Field(Apply(callee), name);
            return IE.MethodCall(Apply(callee), name, ApplyAll(args));
        }
    }

    private class ProjectAnalysisVisitor : ExpressionVisitor
    {
        private readonly LookupTable _l;
        private readonly ParameterExpression _param;

        public HashSet<PropertyInfo> Accesses { get; } = new();
        public bool Escapes { get; private set; } = false;

        public ProjectAnalysisVisitor(MappingContext ctx, ParameterExpression param)
        {
            _l = new LookupTable(ctx);
            _param = param;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // FIXME handle chaining
            if (node.Expression == _param &&
                node.Member is PropertyInfo prop &&
                _l.HasField(prop, node.Expression))
            {
                Accesses.Add(prop);
                return node;
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // FIXME(matt) handle these by checking arg FQL purity
            return base.VisitMethodCall(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == _param)
            {
                Escapes = true;
                return node;
            }

            return base.VisitParameter(node);
        }
    }

    private class ProjectRewriteVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _param;
        private readonly Mapping.FieldInfo[] _fields;
        private readonly Expression[] _fieldAccesses;

        public ProjectRewriteVisitor(
            ParameterExpression doc,
            Mapping.FieldInfo[] fields,
            ParameterExpression projected)
        {
            var accesses = new Expression[fields.Length];

            for (var i = 0; i < fields.Length; i++)
            {
                var idx = Expression.Constant(i);
                accesses[i] = Expression.ArrayIndex(projected, new Expression[] { idx });
            }

            _param = doc;
            _fields = fields;
            _fieldAccesses = accesses;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression == _param)
            {
                var prop = node.Member as PropertyInfo;
                var idx = -1;
                Debug.Assert(prop is not null);

                for (var i = 0; idx < 0 && i < _fields.Length; i++)
                {
                    if (_fields[i].Property == prop)
                    {
                        idx = i;
                    }
                }

                Debug.Assert(idx >= 0);

                return _fieldAccesses[idx];
            }

            return base.VisitMember(node);
        }
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
