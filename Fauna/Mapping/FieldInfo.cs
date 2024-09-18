using System.Reflection;
using Fauna.Serialization;

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

    public FieldType FieldType { get; }

    /// <summary>
    /// The <see cref="Type"/> that the field should deserialize into.
    /// </summary>
    public Type Type { get; }
    /// <summary>
    /// Whether the field is nullable.
    /// </summary>
    public bool IsNullable { get; }

    private MappingContext _ctx;
    private ISerializer? _serializer;

    internal FieldInfo(MappingContext ctx, FieldAttribute attr, PropertyInfo prop, FieldType fieldType)
    {
        var nullCtx = new NullabilityInfoContext();
        var nullInfo = nullCtx.Create(prop);

        Name = attr._name ?? FieldName.Canonical(prop.Name);
        FieldType = fieldType;
        Property = prop;
        Type = prop.PropertyType;
        IsNullable = nullInfo.WriteState is NullabilityState.Nullable;
        _ctx = ctx;
    }

    internal ISerializer Serializer
    {
        get
        {
            lock (_ctx)
            {
                if (_serializer is null)
                {
                    _serializer = Serialization.Serializer.Generate(_ctx, Type);
                    if (IsNullable && (!_serializer.GetType().IsGenericType ||
                                       (_serializer.GetType().IsGenericType &&
                                        _serializer.GetType().GetGenericTypeDefinition() !=
                                        typeof(NullableStructSerializer<>))))
                    {
                        var serType = typeof(NullableSerializer<>).MakeGenericType(new[] { Type });
                        var ser = Activator.CreateInstance(serType, new[] { _serializer });
                        _serializer = (ISerializer)ser!;
                    }
                }

                return _serializer;
            }
        }
    }
}
