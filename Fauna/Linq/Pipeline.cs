using Fauna.Serialization;
using System.Linq.Expressions;

namespace Fauna.Linq;

public enum PipelineMode
{
    Query, // "pure" query. no local processing required (except deserialization)
    Project, // elements have local projection.
    SetLoad, // post-processing on loaded set required
    Scalar, // final, non-enum result: no more transformations allowed
}

internal readonly record struct Pipeline(
    PipelineMode Mode,
    IntermediateExpr Query,
    Type ElemType,
    IDeserializer? ElemDeserializer,
    LambdaExpression? ProjectExpr)
{
    public PipelineExecutor GetExec(DataContext ctx)
    {
        var query = Query.Build();
        var deser = ElemDeserializer ?? Deserializer.Generate(ctx.MappingCtx, ElemType);
        var proj = ProjectExpr is null ? null : ProjectExpr.Compile();
        return PipelineExecutor.Create(ctx, query, deser, proj, Mode);
    }
}
