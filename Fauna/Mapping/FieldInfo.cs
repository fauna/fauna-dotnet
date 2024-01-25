using Fauna.Mapping.Attributes;
using Fauna.Serialization;
using System.Reflection;

namespace Fauna.Mapping;

public sealed class FieldInfo
{
    public readonly string Name;
    public readonly PropertyInfo Property;
    public readonly FaunaType? FaunaTypeHint;
    public readonly Type Type;
    public readonly bool IsNullable;
    private readonly MappingContext _ctx;
    private IDeserializer? _deserializer;

    internal FieldInfo(MappingContext ctx, FieldAttribute attr, PropertyInfo prop)
    {
        var nullCtx = new NullabilityInfoContext();
        var nullInfo = nullCtx.Create(prop);

        Name = attr.Name ?? FieldName.Canonical(prop.Name);
        FaunaTypeHint = attr.Type;
        Property = prop;
        Type = prop.PropertyType;
        IsNullable = nullInfo.WriteState is NullabilityState.Nullable;
        _ctx = ctx;
    }

    internal IDeserializer Deserializer
    {
        get
        {
            lock (_ctx)
            {
                if (_deserializer is null)
                {
                    _deserializer = Fauna.Serialization.Deserializer.Generate(_ctx, Type);
                    if (IsNullable)
                    {
                        _deserializer = new NullableDeserializer(_deserializer);
                    }
                }

                return _deserializer;
            }
        }
    }
}
