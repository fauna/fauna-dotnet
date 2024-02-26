using Fauna.Mapping.Attributes;
using Fauna.Serialization;
using System.Collections.Immutable;
using System.Reflection;

namespace Fauna.Mapping;

/// <summary>
/// A class that encapsulates the class mapping, serialization, and deserialization of a Fauna object, including documents.
/// </summary>
public sealed class MappingInfo
{
    /// <summary>
    /// The associated type.
    /// </summary>
    public Type Type { get; }
    /// <summary>
    /// A read-only list of <see cref="FieldInfo"/> representing the object.
    /// </summary>
    public IReadOnlyList<FieldInfo> Fields { get; }
    /// <summary>
    /// A read-only dictionary of <see cref="FieldInfo"/> representing the object.
    /// </summary>
    public IReadOnlyDictionary<string, FieldInfo> FieldsByName { get; }

    internal bool ShouldEscapeObject { get; }
    internal IClassDeserializer Deserializer { get; }

    internal MappingInfo(MappingContext ctx, Type ty)
    {
        ctx.Add(ty, this);
        Type = ty;

        var objAttr = ty.GetCustomAttribute<ObjectAttribute>();
        var hasAttributes = objAttr != null;
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
