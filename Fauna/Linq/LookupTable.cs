using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Util;
using System.Linq.Expressions;
using System.Reflection;

namespace Fauna.Linq;

// TODO(matt) reconcile/merge this behavior with MappingCtx
internal class LookupTable
{
    public record class Result(string Name, IDeserializer Deserializer, Type Type);
    private static Result R(string name, IDeserializer deser, Type ty) => new Result(name, deser, ty);

    private readonly MappingContext _ctx;

    public LookupTable(MappingContext ctx)
    {
        _ctx = ctx;
    }

    public Result? FieldLookup(PropertyInfo prop, Expression callee)
    {
        if (_ctx.TryGetBaseType(callee.Type, out var info))
        {
            var field = info.Fields.FirstOrDefault(f => f.Property == prop);
            return field is null ? null : R(field.Name, field.Deserializer, field.Type);
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
            "Length" => R("length", Deserializer.Generate<int>(_ctx), typeof(int)),
            "EndsWith" => R("endsWith", Deserializer.Generate<bool>(_ctx), typeof(int)),
            "StartsWith" => R("startsWith", Deserializer.Generate<bool>(_ctx), typeof(int)),
            _ => null,
        };
}