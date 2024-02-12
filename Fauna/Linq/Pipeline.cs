using Fauna.Serialization;
using Fauna.Util;
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
    Func<object[], Query> GetQuery,
    IDeserializer Deserializer,
    Func<object[], Delegate>? GetProjector,
    PipelineMode Mode)
{
    public static PipelineExecutor Get(DataContext ctx, Expression expr)
    {
        var closures = Expressions.FindAllClosures(expr);
        var builder = new PipelineBuilder(ctx, closures, expr);
        var pl = builder.Build();
        return pl.GetExec(ctx, closures);
    }

    public PipelineExecutor GetExec(DataContext ctx, object[] vars) =>
        PipelineExecutor.Create(ctx, GetQuery(vars), Deserializer, Proj(vars), Mode);

    private Delegate? Proj(object[] vars) => GetProjector is null ? null : GetProjector(vars);
}
