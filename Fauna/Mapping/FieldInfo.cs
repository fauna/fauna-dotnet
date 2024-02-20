using Fauna.Mapping.Attributes;
using Fauna.Serialization;
using System.Reflection;

namespace Fauna.Mapping;

public sealed class FieldInfo
{
    public string Name { get; }
    public PropertyInfo Property { get; }
    public FaunaType? FaunaTypeHint { get; }
    public Type Type { get; }
    public bool IsNullable { get; }

    private MappingContext _ctx;
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
                        var deserType = typeof(NullableDeserializer<>).MakeGenericType(new[] { Type });
                        var deser = Activator.CreateInstance(deserType, new[] { _deserializer });
                        _deserializer = (IDeserializer)deser!;
                    }
                }

                return _deserializer;
            }
        }
    }
}
