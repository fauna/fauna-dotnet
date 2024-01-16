using Fauna.Types;

namespace Fauna.Serialization;

public static class Deserializer
{
    public static IDeserializer<object?> Dynamic = DynamicDeserializer.Singleton;

    public static IDeserializer<T> Generate<T>(SerializationContext context)
    {
        var targetType = typeof(T);
        var deser = (IDeserializer<T>)Generate(context, targetType);
        return deser;
    }

    public static IDeserializer<T?> GenerateNullable<T>(SerializationContext context)
    {
        var targetType = typeof(T);
        var deser = (IDeserializer<T>)Generate(context, targetType);
        var nullable = new NullableDeserializer<T>(deser);
        return nullable;
    }

    private static readonly CheckedDeserializer<string> _string = new();
    private static readonly CheckedDeserializer<int> _int = new();
    private static readonly CheckedDeserializer<long> _long = new();
    private static readonly CheckedDeserializer<double> _double = new();
    private static readonly CheckedDeserializer<DateTime> _dateTime = new();
    private static readonly CheckedDeserializer<bool> _bool = new();
    private static readonly CheckedDeserializer<Module> _module = new();
    private static readonly CheckedDeserializer<Document> _doc = new();
    private static readonly CheckedDeserializer<NamedDocument> _namedDoc = new();
    private static readonly CheckedDeserializer<DocumentRef> _docRef = new();
    private static readonly CheckedDeserializer<NullDocumentRef> _nullDocRef = new();
    private static readonly CheckedDeserializer<NamedDocumentRef> _namedDocRef = new();
    private static readonly CheckedDeserializer<NullNamedDocumentRef> _nullNamedDocRef = new();

    private static object Generate(SerializationContext context, Type targetType)
    {
        if (targetType == typeof(object)) return DynamicDeserializer.Singleton;
        if (targetType == typeof(string)) return _string;
        if (targetType == typeof(int)) return _int;
        if (targetType == typeof(long)) return _long;
        if (targetType == typeof(double)) return _double;
        if (targetType == typeof(DateTime)) return _dateTime;
        if (targetType == typeof(bool)) return _bool;
        if (targetType == typeof(Module)) return _module;
        if (targetType == typeof(Document)) return _doc;
        if (targetType == typeof(NamedDocument)) return _namedDoc;
        if (targetType == typeof(DocumentRef)) return _docRef;
        if (targetType == typeof(NullDocumentRef)) return _nullDocRef;
        if (targetType == typeof(NamedDocumentRef)) return _namedDocRef;
        if (targetType == typeof(NullNamedDocumentRef)) return _nullNamedDocRef;

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

            return deser!;
        }

        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var elemType = targetType.GetGenericArguments()[0];
            var elemDeserializer = Generate(context, elemType);

            var deserType = typeof(ListDeserializer<>).MakeGenericType(new[] { elemType });
            var deser = Activator.CreateInstance(deserType, new[] { elemDeserializer });

            return deser!;
        }

        if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Page<>))
        {
            var elemType = targetType.GetGenericArguments()[0];
            var elemDeserializer = Generate(context, elemType);

            var deserType = typeof(PageDeserializer<>).MakeGenericType(new[] { elemType });
            var deser = Activator.CreateInstance(deserType, new[] { elemDeserializer });

            return deser!;
        }

        if (targetType.IsClass && !targetType.IsGenericType)
        {
            var fieldMap = context.GetFieldMap(targetType);

            var deserType = typeof(ClassDeserializer<>).MakeGenericType(new[] { targetType });
            var deser = Activator.CreateInstance(deserType, new object[] { fieldMap });

            return deser!;
        }

        throw new ArgumentException($"Unsupported deserialization target type {targetType}");
    }
}
