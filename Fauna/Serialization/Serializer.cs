using System.Runtime.CompilerServices;
using Fauna.Mapping;
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

    private static readonly DynamicSerializer s_object = DynamicSerializer.Singleton;
    private static readonly StringSerializer s_string = new();
    private static readonly ByteSerializer s_byte = new();
    private static readonly SByteSerializer s_sbyte = new();
    private static readonly ShortSerializer s_short = new();
    private static readonly UShortSerializer s_ushort = new();
    private static readonly IntSerializer s_int = new();
    private static readonly UIntSerializer s_uint = new();
    private static readonly LongSerializer s_long = new();
    private static readonly FloatSerializer s_float = new();
    private static readonly DoubleSerializer s_double = new();
    private static readonly DateOnlySerializer s_dateOnly = new();
    private static readonly DateTimeSerializer s_dateTime = new();
    private static readonly DateTimeOffsetSerializer s_dateTimeOffset = new();
    private static readonly BooleanSerializer s_bool = new();
    private static readonly ModuleSerializer s_module = new();
    private static readonly StreamSerializer s_stream = new();
    private static readonly QuerySerializer s_query = new();
    private static readonly QueryExprSerializer s_queryExpr = new();
    private static readonly QueryLiteralSerializer s_queryLiteral = new();
    private static readonly QueryArrSerializer s_queryArr = new();
    private static readonly QueryObjSerializer s_queryObj = new();
    private static readonly QueryValSerializer s_queryVal = new();


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

        if (IsAnonymousType(targetType))
        {
            var info = context.GetInfo(targetType);
            return info.ClassSerializer;
        }

        if (targetType == typeof(object)) return s_object;
        if (targetType == typeof(string)) return s_string;
        if (targetType == typeof(byte)) return s_byte;
        if (targetType == typeof(sbyte)) return s_sbyte;
        if (targetType == typeof(short)) return s_short;
        if (targetType == typeof(ushort)) return s_ushort;
        if (targetType == typeof(int)) return s_int;
        if (targetType == typeof(uint)) return s_uint;
        if (targetType == typeof(long)) return s_long;
        if (targetType == typeof(float)) return s_float;
        if (targetType == typeof(double)) return s_double;
        if (targetType == typeof(DateOnly)) return s_dateOnly;
        if (targetType == typeof(DateTime)) return s_dateTime;
        if (targetType == typeof(DateTimeOffset)) return s_dateTimeOffset;
        if (targetType == typeof(bool)) return s_bool;
        if (targetType == typeof(Module)) return s_module;
        if (targetType == typeof(Stream)) return s_stream;
        if (targetType == typeof(Query)) return s_query;
        if (targetType == typeof(QueryExpr)) return s_queryExpr;
        if (targetType == typeof(QueryLiteral)) return s_queryLiteral;
        if (targetType == typeof(QueryArr)) return s_queryArr;
        if (targetType == typeof(QueryObj)) return s_queryObj;
        if (targetType == typeof(QueryVal)) return s_queryVal;

        if (targetType.IsGenericType)
        {
            var args = targetType.GetGenericArguments();
            var blocked = args.Where(arg => arg.GetInterfaces().Contains(typeof(IQueryFragment)));

            if (blocked.Any())
            {
                // throw new ArgumentException(
                //     $"Query types ({string.Join(", ", blocked.Select(x => x.FullName))}) are not supported as " +
                //     $"generic type arguments; use {nameof(QueryArr)} or {nameof(QueryObj)} when composing complex queries.");
            }

            if (targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (args.Length == 1)
                {
                    var inner = (ISerializer)Generate(context, args[0]);
                    var serType = typeof(NullableStructSerializer<>).MakeGenericType(new[] { args[0] });
                    object? ser = Activator.CreateInstance(serType, new[] { inner });

                    return (ISerializer)ser!;
                }

                throw new ArgumentException($"Unsupported nullable type. Generic arguments > 1: {args}");
            }

            if (targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = args[0];
                var valueType = args[1];

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
                var elemType = args[0];
                var elemSerializer = Generate(context, elemType);

                var serType = typeof(ListSerializer<>).MakeGenericType(new[] { elemType });
                object? ser = Activator.CreateInstance(serType, new[] { elemSerializer });

                return (ISerializer)ser!;
            }

            if (targetType.GetGenericTypeDefinition() == typeof(Page<>))
            {
                var elemType = args[0];
                var elemSerializer = Generate(context, elemType);

                var serType = typeof(PageSerializer<>).MakeGenericType(new[] { elemType });
                object? ser = Activator.CreateInstance(serType, new[] { elemSerializer });

                return (ISerializer)ser!;
            }

            if (targetType.GetGenericTypeDefinition() == typeof(BaseRef<>))
            {
                var docType = args[0];
                var docSerializer = Generate(context, docType);

                var serType = typeof(BaseRefSerializer<>).MakeGenericType(new[] { docType });
                object? ser = Activator.CreateInstance(serType, new[] { docSerializer });

                return (ISerializer)ser!;
            }

            if (targetType.GetGenericTypeDefinition() == typeof(Ref<>))
            {
                var docType = args[0];
                var docSerializer = Generate(context, docType);

                var serType = typeof(RefSerializer<>).MakeGenericType(new[] { docType });
                object? ser = Activator.CreateInstance(serType, new[] { docSerializer });

                return (ISerializer)ser!;
            }

            if (targetType.GetGenericTypeDefinition() == typeof(NamedRef<>))
            {
                var docType = args[0];
                var docSerializer = Generate(context, docType);

                var serType = typeof(NamedRefSerializer<>).MakeGenericType(new[] { docType });
                object? ser = Activator.CreateInstance(serType, new[] { docSerializer });

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

    private static bool IsAnonymousType(this Type type)
    {
        bool hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
        bool nameContainsAnonymousType = type?.FullName?.Contains("AnonymousType") ?? false;
        return hasCompilerGeneratedAttribute && nameContainsAnonymousType;
    }
}
