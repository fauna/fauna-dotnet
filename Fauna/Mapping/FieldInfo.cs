using Fauna.Mapping.Attributes;
using Fauna.Serialization;
using System.Reflection;

namespace Fauna.Mapping;

/// <summary>
/// A class that encapsulates the field mapping, serialization, and deserialization of a particular field in Fauna.
/// </summary>
public sealed class FieldInfo
{
    /// <summary>
    /// The name of the field.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// The property info of an associated class.
    /// </summary>
    public PropertyInfo Property { get; }
    /// <summary>
    /// Indicates which fauna type the value should serialize into.
    /// </summary>
    public FaunaType? FaunaTypeHint { get; }
    /// <summary>
    /// The <see cref="Type"/> that the field should deserialize into.
    /// </summary>
    public Type Type { get; }
    /// <summary>
    /// Whether the field is nullable.
    /// </summary>
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
                    if (IsNullable && _deserializer.GetType().GetGenericTypeDefinition() != typeof(NullableStructDeserializer<>))
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
