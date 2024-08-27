using Fauna.Mapping;
using Fauna.Serialization;
using Fauna.Types;
using Stream = Fauna.Types.Stream;

namespace Fauna.Serialization;

/// <summary>
/// Represents methods for serializing objects to and from Fauna's value format.
/// </summary>
public static class Serializer
{
    /// <summary>
    /// The dynamic data serializer.
    /// </summary>
    public static ISerializer<object?> Dynamic => DynamicSerializer.Singleton;

    private static readonly Dictionary<Type, ISerializer> _reg = new();

    internal static readonly HashSet<string> Tags = new()
    {
        "@int", "@long", "@double", "@date", "@time", "@mod", "@stream", "@ref", "@doc", "@set", "@object"
    };

    private static readonly CheckedSerializer<object> _object = new();
    private static readonly StringSerializer _string = new();
    private static readonly ByteSerializer _byte = new();
    private static readonly SByteSerializer _sbyte = new();
    private static readonly ShortSerializer _short = new();
    private static readonly UShortSerializer _ushort = new();
    private static readonly IntSerializer _int = new();
    private static readonly UIntSerializer _uint = new();
    private static readonly LongSerializer _long = new();
    private static readonly FloatSerializer _float = new();
    private static readonly DoubleSerializer _double = new();
    private static readonly DateOnlySerializer _dateOnly = new();
    private static readonly DateTimeSerializer _dateTime = new();
    private static readonly DateTimeOffsetSerializer _dateTimeOffset = new();
    private static readonly BooleanSerializer _bool = new();
    private static readonly ModuleSerializer _module = new();
    private static readonly StreamSerializer _stream = new();
    private static readonly DocumentSerializer<Document> _doc = new();
    private static readonly DocumentSerializer<NamedDocument> _namedDoc = new();
    private static readonly DocumentSerializer<Ref> _docRef = new();
    private static readonly DocumentSerializer<NamedRef> _namedDocRef = new();

    /// <summary>
    /// Generates a serializer for the specified non-nullable .NET type.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="context">The serialization context.</param>
    /// <returns>An <see cref="ISerializer{T}"/>.</returns>
    public static ISerializer<T> Generate<T>(MappingContext context) where T : notnull
    {
        var targetType = typeof(T);
        var ser = (ISerializer<T>)Generate(context, targetType);
        return ser;
    }

    /// <summary>
    /// Generates a serializer for the specified non-nullable .NET type.
    /// </summary>
    /// <param name="context">The serialization context.</param>
    /// <param name="targetType">The type of the object to serialize.</typeparam>
    /// <returns>An <see cref="ISerializer"/>.</returns>
    public static ISerializer Generate(MappingContext context, Type targetType)
    {
        if (_reg.TryGetValue(targetType, out var s)) return s;
        if (targetType == typeof(object)) return _object;
        if (targetType == typeof(string)) return _string;
        if (targetType == typeof(byte)) return _byte;
        if (targetType == typeof(sbyte)) return _sbyte;
        if (targetType == typeof(short)) return _short;
        if (targetType == typeof(ushort)) return _ushort;
        if (targetType == typeof(int)) return _int;
        if (targetType == typeof(uint)) return _uint;
        if (targetType == typeof(long)) return _long;
        if (targetType == typeof(float)) return _float;
        if (targetType == typeof(double)) return _double;
        if (targetType == typeof(DateOnly)) return _dateOnly;
        if (targetType == typeof(DateTime)) return _dateTime;
        if (targetType == typeof(DateTimeOffset)) return _dateTimeOffset;
        if (targetType == typeof(bool)) return _bool;
        if (targetType == typeof(Module)) return _module;
        if (targetType == typeof(Stream)) return _stream;
        if (targetType == typeof(Document)) return _doc;
        if (targetType == typeof(NamedDocument)) return _namedDoc;
        if (targetType == typeof(Ref)) return _docRef;
        if (targetType == typeof(NamedRef)) return _namedDocRef;

        if (targetType.IsGenericType)
        {
            if (targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var args = targetType.GetGenericArguments();
                if (args.Length == 1)
                {
                    var inner = (ISerializer)Generate(context, args[0]);
                    var serType = typeof(NullableStructSerializer<>).MakeGenericType(new[] { args[0] });
                    object? ser = Activator.CreateInstance(serType, new[] { inner });

                    return (ISerializer)ser!;
                }

                throw new ArgumentException($"Unsupported nullable type. Generic arguments > 1: {args}");
            }

            if (targetType.GetGenericTypeDefinition() == typeof(NullableDocument<>) ||
                targetType.GetGenericTypeDefinition() == typeof(NonNullDocument<>) ||
                targetType.GetGenericTypeDefinition() == typeof(NullDocument<>))
            {
                var argTypes = targetType.GetGenericArguments();
                var valueType = argTypes[0];
                var serType = typeof(NullableDocumentSerializer<>).MakeGenericType(new[] { valueType });
                object? ser = Activator.CreateInstance(serType);
                return (ISerializer)ser!;
            }

            if (targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var argTypes = targetType.GetGenericArguments();
                var keyType = argTypes[0];
                var valueType = argTypes[1];

                if (keyType != typeof(string))
                    throw new ArgumentException(
                        $"Unsupported Dictionary key type. Key must be of type string, but was a {keyType}");

                var valueSerializer = Generate(context, valueType);

                var serType = typeof(DictionarySerializer<>).MakeGenericType(new[] { valueType });
                object? ser = Activator.CreateInstance(serType, new[] { valueSerializer });

                return (ISerializer)ser!;
            }

            if (targetType.GetGenericTypeDefinition() == typeof(List<>) || targetType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var elemType = targetType.GetGenericArguments()[0];
                var elemSerializer = Generate(context, elemType);

                var serType = typeof(ListSerializer<>).MakeGenericType(new[] { elemType });
                object? ser = Activator.CreateInstance(serType, new[] { elemSerializer });

                return (ISerializer)ser!;
            }

            if (targetType.GetGenericTypeDefinition() == typeof(Page<>))
            {
                var elemType = targetType.GetGenericArguments()[0];
                var elemSerializer = Generate(context, elemType);

                var serType = typeof(PageSerializer<>).MakeGenericType(new[] { elemType });
                object? ser = Activator.CreateInstance(serType, new[] { elemSerializer });

                return (ISerializer)ser!;
            }

            if (targetType.IsGenericType && targetType.Name.Contains("AnonymousType"))
            {
                return DynamicSerializer.Singleton;
            }
        }


        if (targetType.IsClass)
        {
            var info = context.GetInfo(targetType);
            return info.ClassSerializer;
        }

        throw new ArgumentException($"Unsupported deserialization target type {targetType}");
    }

    /// <summary>
    /// Generates a serializer which returns values of the specified .NET type, or the default if the underlying query value is null.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="context">The serialization context.</param>
    /// <returns>An <see cref="ISerializer{T}"/>.</returns>
    public static ISerializer<T?> GenerateNullable<T>(MappingContext context)
    {
        var targetType = typeof(T);
        var ser = (ISerializer<T>)Generate(context, targetType);
        return new NullableSerializer<T>(ser);
    }

    /// <summary>
    /// Generates a serializer which returns values of the specified .NET type, or the default if the underlying query value is null.
    /// </summary>
    /// <param name="context">The serialization context.</param>
    /// <param name="targetType">The type of the object to serialize.</typeparam>
    /// <returns>An <see cref="ISerializer"/>.</returns>
    public static ISerializer GenerateNullable(MappingContext context, Type targetType)
    {
        var inner = (ISerializer)Generate(context, targetType);
        var serType = typeof(NullableSerializer<>).MakeGenericType(new[] { targetType });
        var ser = Activator.CreateInstance(serType, new[] { inner });

        return (ISerializer)ser!;
    }

    /// <summary>
    /// Registers a serializer for a type. This serializer will take precedence over the default serializer for the that type.
    /// </summary>
    /// <param name="t">The type to associate with the serializer.</param>
    /// <param name="s">The serializer.</param>
    /// <exception cref="ArgumentException">Throws if a serializer is already registered for the type.</exception>
    public static void Register(Type t, ISerializer s)
    {
        if (!_reg.TryAdd(t, s)) throw new ArgumentException($"Serializer for type `{t}` already registered");
    }

    /// <summary>
    /// Registers a generic serializer. This serializer will take precedence over the default serializer for the that type.
    /// </summary>
    /// <param name="s">The generic serializer.</param>
    /// <exception cref="ArgumentException">Throws if a serializer is already registered for the type.</exception>
    public static void Register<T>(ISerializer<T> s)
    {
        var success = false;
        foreach (var i in s.GetType().GetInterfaces())
        {
            if (!i.IsGenericType || i.GetGenericTypeDefinition() != typeof(ISerializer<>)) continue;

            var t = i.GetGenericArguments()[0];
            success = _reg.TryAdd(t, s);
            if (!success) throw new ArgumentException($"Serializer for type `{t}` already registered");
            break;
        }

        if (!success) throw new ArgumentException($"Could not infer associated type for `{s.GetType()}`. Use Register(type, serializer).");
    }

    /// <summary>
    /// Deregisters a serializer for a type. If no serializer was registered, no-op.
    /// </summary>
    /// <param name="t">The associated type to deregister.</param>
    public static void Deregister(Type t)
    {
        if (_reg.ContainsKey(t)) _reg.Remove(t);
    }
}
