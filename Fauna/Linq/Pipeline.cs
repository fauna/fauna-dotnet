using System.Linq.Expressions;
using Fauna.Serialization;

namespace Fauna.Linq;

/// <summary>
/// The mode of the query pipeline.
/// </summary>
public enum PipelineMode
{
    /// <summary>
    /// When a "pure" query, no local processing is required except deserialization.
    /// </summary>
    Query,
    /// <summary>
    /// When elements have local projection.
    /// </summary>
    Project,
    /// <summary>
    /// When post-processing on the loaded set is required.
    /// </summary>
    SetLoad,
    /// <summary>
    /// When there is a final, non-enum result, no more transformations are allowed.
    /// </summary>
    Scalar,
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
             Serializer.GenerateNullable(ctx.MappingCtx, ElemType) :
             Serializer.Generate(ctx.MappingCtx, ElemType));

        var proj = ProjectExpr?.Compile();

        return IPipelineExecutor.Create(ctx, Query, ser, proj, Mode);
    }
}
