using Fauna.Mapping.Attributes;
using Fauna.Serialization;
using System.Collections.Immutable;
using System.Reflection;

namespace Fauna.Mapping;

public sealed class MappingInfo
{
    public readonly Type Type;
    public readonly string? Collection;
    public readonly bool IsCollection;
    public readonly IReadOnlyList<FieldInfo> Fields;
    public readonly IReadOnlyDictionary<string, FieldInfo> FieldsByName;

    internal readonly bool ShouldEscapeObject;
    internal readonly IClassDeserializer Deserializer;

    internal MappingInfo(MappingContext ctx, Type ty)
    {
        ctx.Add(ty, this);
        Type = ty;

        var collAttr = ty.GetCustomAttribute<CollectionAttribute>();
        var objAttr = ty.GetCustomAttribute<ObjectAttribute>();
        var hasAttributes = collAttr != null || objAttr != null;

        Collection = collAttr?.Name;
        IsCollection = Collection is not null;

        var fields = new List<FieldInfo>();
        var byName = new Dictionary<string, FieldInfo>();

        foreach (var prop in ty.GetProperties())
        {
            var attr = hasAttributes ?
                prop.GetCustomAttribute<FieldAttribute>() :
                new FieldAttribute();

            if (attr is null) continue;

            var info = new FieldInfo(ctx, attr, prop);

            if (byName.ContainsKey(info.Name))
                throw new ArgumentException($"Duplicate field name {info.Name} in {ty}");

            fields.Add(info);
            byName[info.Name] = info;
        }

        ShouldEscapeObject = Serializer.Tags.Overlaps(byName.Values.Select(i => i.Name));
        Fields = fields.ToImmutableList();
        FieldsByName = byName.ToImmutableDictionary();

        var deserType = typeof(ClassDeserializer<>).MakeGenericType(new[] { ty });
        Deserializer = (IClassDeserializer)Activator.CreateInstance(deserType, new[] { this })!;
    }
}
