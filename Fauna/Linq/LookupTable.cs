using Fauna.Mapping;
using Fauna.Serialization;
using System.Linq.Expressions;
using System.Reflection;

namespace Fauna.Linq;

// TODO(matt) reconcile/merge this behavior with MappingCtx
internal record struct LookupTable(MappingContext Ctx)
{
    public record class Result(string Name, ICodec Codec, Type Type);
    private static Result R(string name, ICodec deser, Type ty) => new Result(name, deser, ty);

    public Result? FieldLookup(PropertyInfo prop, Expression callee)
    {
        if (Ctx.TryGetBaseType(callee.Type, out var info))
        {
            var field = info.Fields.FirstOrDefault(f => f.Property == prop);
            return field is null ? null : R(field.Name, field.Codec, field.Type);
        }

        return Table(prop, callee);
    }

    public Result? MethodLookup(MethodInfo method, Expression callee) =>
        Table(method, callee);

    public bool HasField(PropertyInfo prop, Expression callee) =>
        FieldLookup(prop, callee) is not null;

    public bool HasMethod(MethodInfo method, Expression callee) =>
        MethodLookup(method, callee) is not null;


    // built-ins

    private Result? Table(MemberInfo member, Expression callee) =>
        callee.Type.Name switch
        {
            "string" => StringTable(member, callee),
            _ => null,
        };

    private Result? StringTable(MemberInfo member, Expression callee) =>
        member.Name switch
        {
            "Length" => R("length", Codec.Generate<int>(Ctx), typeof(int)),
            "EndsWith" => R("endsWith", Codec.Generate<bool>(Ctx), typeof(int)),
            "StartsWith" => R("startsWith", Codec.Generate<bool>(Ctx), typeof(int)),
            _ => null,
        };
}
