using System.Reflection;
using Fauna.Serialization.Attributes;

namespace Fauna.Serialization;

/// <summary>
/// Represents the context for serialization and deserialization operations within Fauna.
/// </summary>
public class SerializationContext
{
    private readonly Dictionary<Type, Dictionary<string, FieldAttribute>> _registry = new();

    /// <summary>
    /// Get a deserializer for values of type T.
    /// </summary>
    /// <typeparam name="T">The result type of the returned deserializer.</typeparam>
    /// <returns>An <see cref="IDeserializer{T}"/> which deserializes values of type T from their corresponding query results.</returns>
    public IDeserializer<T> GetDeserializer<T>() where T : notnull
    {
        // FIXME(matt) cache this
        return Deserializer.Generate<T>(this);
    }

    /// <summary>
    /// Get a deserializer for values of type ty.
    /// </summary>
    /// <param name="ty">The result type of the returned deserializer.</typeparam>
    /// <returns>An <see cref="IDeserializer"/> which deserializes values of type `ty` from their corresponding query results.</returns>
    public IDeserializer GetDeserializer(Type ty)
    {
        // FIXME(matt) cache this
        return Deserializer.Generate(this, ty);
    }

    /// <summary>
    /// Retrieves the mapping of property names to their corresponding <see cref="FieldAttribute"/> for a given .NET type.
    /// </summary>
    /// <param name="t">The type for which the field map is requested.</param>
    /// <returns>A dictionary where keys are property names and values are the corresponding <see cref="FieldAttribute"/> instances.</returns>
    public Dictionary<string, FieldAttribute> GetFieldMap(Type t)
    {
        if (_registry.TryGetValue(t, out var fieldMap))
        {
            return fieldMap;
        }

        var props = t.GetProperties();
        var newFieldMap = new Dictionary<string, FieldAttribute>();
        var hasAttributes = t.GetCustomAttribute<FaunaObjectAttribute>() != null;

        foreach (var prop in props)
        {
            FieldAttribute attr;
            if (hasAttributes)
            {
                var a = prop.GetCustomAttribute<FieldAttribute>();
                if (a is null) continue;
                attr = a;
            }
            else
            {
                attr = new FieldAttribute(prop.Name);
            }

            attr.Info = prop;
            newFieldMap[attr.Name] = attr;
        }


        _registry[t] = newFieldMap;

        return newFieldMap;
    }
}
