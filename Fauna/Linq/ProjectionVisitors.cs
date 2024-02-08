using Fauna.Mapping;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Fauna.Linq;

internal class ProjectionAnalysisVisitor : ExpressionVisitor
{
    private readonly LookupTable _l;
    private readonly ParameterExpression _param;

    public HashSet<PropertyInfo> Accesses { get; } = new();
    public bool Escapes { get; private set; } = false;

    public ProjectionAnalysisVisitor(MappingContext ctx, ParameterExpression param)
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

internal class ProjectionRewriteVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _param;
    private readonly PropertyInfo[] _props;
    private readonly Expression[] _fieldAccesses;

    public ProjectionRewriteVisitor(
        ParameterExpression doc,
        PropertyInfo[] props,
        ParameterExpression projected)
    {
        var accesses = new Expression[props.Length];

        for (var i = 0; i < props.Length; i++)
        {
            accesses[i] = Expression.Convert(
                Expression.ArrayIndex(projected, Expression.Constant(i)),
                props[i].PropertyType);
        }

        _param = doc;
        _props = props;
        _fieldAccesses = accesses;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression == _param)
        {
            var prop = node.Member as PropertyInfo;
            var idx = -1;
            Debug.Assert(prop is not null);

            for (var i = 0; idx < 0 && i < _props.Length; i++)
            {
                if (_props[i] == prop)
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
