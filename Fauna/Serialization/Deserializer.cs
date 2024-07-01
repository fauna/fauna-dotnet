using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

/// <summary>
/// Represents methods for deserializing objects to and from Fauna's value format.
/// </summary>
public static class Deserializer
{
    /// <summary>
    /// The dynamic data deserializer.
    /// </summary>
    public static IDeserializer<object?> Dynamic { get; } = DynamicDeserializer.Singleton;

    private static readonly CheckedDeserializer<object> _object = new();
    private static readonly CheckedDeserializer<string> _string = new();
    private static readonly CheckedDeserializer<int> _int = new();
    private static readonly LongDeserializer _long = new();
    private static readonly ShortDeserializer _short = new();
    private static readonly UShortDeserializer _ushort = new();
    private static readonly CheckedDeserializer<double> _double = new();
    private static readonly CheckedDeserializer<DateOnly> _dateOnly = new();
    private static readonly CheckedDeserializer<DateTime> _dateTime = new();
    private static readonly CheckedDeserializer<bool> _bool = new();
    private static readonly CheckedDeserializer<Module> _module = new();
    private static readonly DocumentDeserializer<Document> _doc = new();
    private static readonly DocumentDeserializer<NamedDocument> _namedDoc = new();
    private static readonly DocumentDeserializer<DocumentRef> _docRef = new();
    private static readonly DocumentDeserializer<NamedDocumentRef> _namedDocRef = new();

    /// <summary>
    /// Generates a deserializer for the specified non-nullable .NET type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="context">The serialization context.</param>
    /// <returns>An <see cref="IDeserializer{T}"/>.</returns>
    public static IDeserializer<T> Generate<T>(MappingContext context) where T : notnull
    {
        var targetType = typeof(T);
        var deser = (IDeserializer<T>)Generate(context, targetType);
        return deser;
    }

    /// <summary>
    /// Generates a deserializer for the specified non-nullable .NET type.
    /// </summary>
    /// <param name="context">The serialization context.</param>
    /// <param name="targetType">The type of the object to deserialize to.</typeparam>
    /// <returns>An <see cref="IDeserializer"/>.</returns>
    public static IDeserializer Generate(MappingContext context, Type targetType)
    {
        if (targetType == typeof(object)) return _object;
        if (targetType == typeof(string)) return _string;
        if (targetType == typeof(short)) return _short;
        if (targetType == typeof(ushort)) return _ushort;
        if (targetType == typeof(int)) return _int;
        if (targetType == typeof(long)) return _long;
        if (targetType == typeof(double)) return _double;
        if (targetType == typeof(DateOnly)) return _dateOnly;
        if (targetType == typeof(DateTime)) return _dateTime;
        if (targetType == typeof(bool)) return _bool;
        if (targetType == typeof(Module)) return _module;
        if (targetType == typeof(Document)) return _doc;
        if (targetType == typeof(NamedDocument)) return _namedDoc;
        if (targetType == typeof(DocumentRef)) return _docRef;
        if (targetType == typeof(NamedDocumentRef)) return _namedDocRef;

        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var args = targetType.GetGenericArguments();
            if (args.Length == 1)
            {
                var inner = (IDeserializer)Generate(context, args[0]);
                var deserType = typeof(NullableStructDeserializer<>).MakeGenericType(new[] { args[0] });
                var deser = Activator.CreateInstance(deserType, new[] { inner });

                return (IDeserializer)deser!;
            }

            throw new ArgumentException($"Unsupported nullable type. Generic arguments > 1: {args}");
        }

        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(NullableDocument<>))
        {
            var argTypes = targetType.GetGenericArguments();
            var valueType = argTypes[0];
            var deserType = typeof(NullableDocumentDeserializer<>).MakeGenericType(new[] { valueType });
            var deser = Activator.CreateInstance(deserType);
            return (IDeserializer)deser!;
        }

        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var argTypes = targetType.GetGenericArguments();
            var keyType = argTypes[0];
            var valueType = argTypes[1];

            if (keyType != typeof(string))
                throw new ArgumentException(
                    $"Unsupported Dictionary key type. Key must be of type string, but was a {keyType}");

            var valueDeserializer = Generate(context, valueType);

            var deserType = typeof(DictionaryDeserializer<>).MakeGenericType(new[] { valueType });
            var deser = Activator.CreateInstance(deserType, new[] { valueDeserializer });

            return (IDeserializer)deser!;
        }

        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var elemType = targetType.GetGenericArguments()[0];
            var elemDeserializer = Generate(context, elemType);

            var deserType = typeof(ListDeserializer<>).MakeGenericType(new[] { elemType });
            var deser = Activator.CreateInstance(deserType, new[] { elemDeserializer });

            return (IDeserializer)deser!;
        }

        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Page<>))
        {
            var elemType = targetType.GetGenericArguments()[0];
            var elemDeserializer = Generate(context, elemType);

            var deserType = typeof(PageDeserializer<>).MakeGenericType(new[] { elemType });
            var deser = Activator.CreateInstance(deserType, new[] { elemDeserializer });

            return (IDeserializer)deser!;
        }

        if (targetType.IsClass && !targetType.IsGenericType)
        {
            var info = context.GetInfo(targetType);
            return info.Deserializer;
        }

        throw new ArgumentException($"Unsupported deserialization target type {targetType}");
    }

    /// <summary>
    /// Generates a deserializer which returns values of the specified .NET type, or the default if the underlying query value is null.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="context">The serialization context.</param>
    /// <returns>An <see cref="IDeserializer{T}"/>.</returns>
    public static IDeserializer<T?> GenerateNullable<T>(MappingContext context)
    {
        var targetType = typeof(T);
        var deser = (IDeserializer<T>)Generate(context, targetType);
        return new NullableDeserializer<T>(deser);
    }

    /// <summary>
    /// Generates a deserializer which returns values of the specified .NET type, or the default if the underlying query value is null.
    /// </summary>
    /// <param name="context">The serialization context.</param>
    /// <param name="targetType">The type of the object to deserialize to.</typeparam>
    /// <returns>An <see cref="IDeserializer"/>.</returns>
    public static IDeserializer GenerateNullable(MappingContext context, Type targetType)
    {
        var inner = (IDeserializer)Generate(context, targetType);
        var deserType = typeof(NullableDeserializer<>).MakeGenericType(new[] { targetType });
        var deser = Activator.CreateInstance(deserType, new[] { inner });

        return (IDeserializer)deser!;
    }
}
