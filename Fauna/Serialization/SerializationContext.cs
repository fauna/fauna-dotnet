using System.Reflection;
using Fauna.Serialization.Attributes;

namespace Fauna.Serialization;

public class SerializationContext
{
    private readonly Dictionary<Type, Dictionary<string, FieldAttribute>> _registry = new();

    public IDeserializer<T> GetDeserializer<T>() where T : notnull
    {
        // FIXME(matt) cache this
        return Deserializer.Generate<T>(this);
    }

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
