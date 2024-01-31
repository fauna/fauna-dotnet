using Fauna.Serialization;
using Fauna.Types;
using Fauna.Util;
using System.Diagnostics;
using System.Linq.Expressions;
using IE = Fauna.Linq.IntermediateExpr;

namespace Fauna.Linq;

internal class PipelineBuilder
{
    protected DataContext DataCtx { get; }
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

    private class QuerySwitch : BaseSwitch<IE>
    {
        public QuerySwitch(PipelineBuilder builder) : base(builder) { }

        private IDeserializer DocPageDeserializer(Type docType)
        {
            var pageType = typeof(Page<>).MakeGenericType(docType);
            return Serialization.Deserializer.Generate(_builder.DataCtx.MappingCtx, pageType);
        }

        protected override IE ConstantExpr(ConstantExpression expr)
        {
            if (expr.Value is DataContext.Collection col)
            {
                _builder.Deserializer = DocPageDeserializer(col.DocType);

                return new IE.Expr($"{col.Name}.all()");
            }
            else if (expr.Value is DataContext.Index idx)
            {
                _builder.Deserializer = DocPageDeserializer(idx.DocType);

                var prefix = $"{idx.Collection.Name}.{idx.Name}";

                if (idx.Args.Length == 0)
                {
                    return new IE.Expr($"{prefix}()");
                }

                IntermediateExpr ret = new IE.Expr($"{prefix}(");

                for (var i = 0; i < idx.Args.Length; i++)
                {
                    if (i > 0) ret = ret.Concat(", ");
                    var arg = idx.Args[i];
                    ret = ret.Concat(new IE.Constant(arg));
                }
                ret = ret.Concat(")");

                return ret;
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
    }

    private class PredicateSwitch : BaseSwitch<IE>
    {
        public PredicateSwitch(PipelineBuilder builder) : base(builder) { }
    }

    private class ProjectSwitch : BaseSwitch<IE>
    {
        public ProjectSwitch(PipelineBuilder builder) : base(builder) { }
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

    // TODO(matt) use an API-specific exception in-line with what other LINQ
    // libraries do.
    private static Exception fail(Expression? expr) =>
        new NotSupportedException($"Unsupported {expr?.NodeType} expression: {expr}");
}
