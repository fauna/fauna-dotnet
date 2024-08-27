using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization.Serializers;

internal class NullableDocumentSerializer<T> : BaseSerializer<NullableDocument<T>> where T : class
{
    private static readonly DocumentSerializer<NullableDocument<Document>> _nullableDoc = new();
    private static readonly DocumentSerializer<NullableDocument<NamedDocument>> _nullableNamedDoc = new();
    private static readonly DocumentSerializer<NullableDocument<Ref>> _nullabelDocRef = new();
    private static readonly DocumentSerializer<NullableDocument<NamedRef>> _nullabelNamedDocRef = new();

    public NullableDocumentSerializer()
    {
    }

    public override NullableDocument<T> Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        if (reader.CurrentTokenType is not (TokenType.StartObject or TokenType.StartRef or TokenType.StartDocument))
            throw new SerializationException(
                $"Unexpected token while deserializing into {typeof(NullableDocument<T>)}: {reader.CurrentTokenType}");

        if (typeof(T) == typeof(Document)) return (_nullableDoc.Deserialize(context, ref reader) as NullableDocument<T>)!;
        if (typeof(T) == typeof(NamedDocument)) return (_nullableNamedDoc.Deserialize(context, ref reader) as NullableDocument<T>)!;
        if (typeof(T) == typeof(Ref)) return (_nullabelDocRef.Deserialize(context, ref reader) as NullableDocument<T>)!;
        if (typeof(T) == typeof(NamedRef)) return (_nullabelNamedDocRef.Deserialize(context, ref reader) as NullableDocument<T>)!;

        var info = context.GetInfo(typeof(T));
        try
        {
            var v = info.ClassSerializer.Deserialize(context, ref reader);
            return new NonNullDocument<T>((v as T)!);
        }
        catch (NullDocumentException e)
        {
            return new NullDocument<T>(e.Id, e.Collection, e.Cause);
        }
    }

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        DynamicSerializer.Singleton.Serialize(context, writer, o);
    }
}
