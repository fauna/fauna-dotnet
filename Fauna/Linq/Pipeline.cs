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
    Query Query,
    IDeserializer Deserializer,
    Delegate? Projector,
    PipelineMode Mode)
{
    public static PipelineExecutor Get(DataContext ctx, Expression expr)
    {
        var builder = new PipelineBuilder(ctx, expr);
        var pl = builder.Build();
        return pl.GetExec(ctx);
    }

    public PipelineExecutor GetExec(DataContext ctx) =>
        PipelineExecutor.Create(ctx, Query, Deserializer, Projector, Mode);
}
