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
    Query Query,
    Type ElemType,
    bool ElemNullable,
    IDeserializer? ElemDeserializer,
    LambdaExpression? ProjectExpr)
{
    public IPipelineExecutor GetExec(DataContext ctx)
    {
        var deser = ElemDeserializer ??
            (ElemNullable ?
             Deserializer.GenerateNullable(ctx.MappingCtx, ElemType) :
             Deserializer.Generate(ctx.MappingCtx, ElemType));

        var proj = ProjectExpr?.Compile();

        return IPipelineExecutor.Create(ctx, Query, deser, proj, Mode);
    }
}
