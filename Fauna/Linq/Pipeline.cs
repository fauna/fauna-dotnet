using System.Linq.Expressions;
using Fauna.Serialization;

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
    ISerializer? ElemSerializer,
    LambdaExpression? ProjectExpr)
{
    public IPipelineExecutor GetExec(DataContext ctx)
    {
        var ser = ElemSerializer ??
            (ElemNullable ?
             SerializerProvider.GenerateNullable(ctx.MappingCtx, ElemType) :
             SerializerProvider.Generate(ctx.MappingCtx, ElemType));

        var proj = ProjectExpr?.Compile();

        return IPipelineExecutor.Create(ctx, Query, ser, proj, Mode);
    }
}
