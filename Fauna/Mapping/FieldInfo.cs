using Fauna.Mapping.Attributes;
using System.Reflection;

namespace Fauna.Mapping;

public sealed class FieldInfo
{
    public readonly string Name;
    public readonly PropertyInfo Property;
    public readonly FaunaType? FaunaTypeHint;
    public readonly Type Type;
    public readonly bool IsNullable;

    internal FieldInfo(MappingContext ctx, FieldAttribute attr, PropertyInfo prop)
    {
        var nullCtx = new NullabilityInfoContext();
        var nullInfo = nullCtx.Create(prop);

        Name = attr.Name ?? CanonicalFieldName(prop);
        FaunaTypeHint = attr.Type;
        Property = prop;
        Type = prop.PropertyType;
        IsNullable = nullInfo.WriteState is NullabilityState.Nullable;
    }

    // C# properties are capitalized whereas Fauna fields are not by convention.
    private static string CanonicalFieldName(PropertyInfo prop) =>
        prop.Name switch
        {
            var name when string.IsNullOrEmpty(name) || char.IsLower(name[0]) => name,
            var name => string.Concat(name[0].ToString().ToLower(), name.AsSpan(1))
        };
}
