using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal class NullableDocumentDeserializer<T> : BaseDeserializer<NullableDocument<T>> where T : class
{
    private static readonly DocumentDeserializer<NullableDocument<Document>> _nullableDoc = new();
    private static readonly DocumentDeserializer<NullableDocument<NamedDocument>> _nullableNamedDoc = new();
    private static readonly DocumentDeserializer<NullableDocument<Ref>> _nullabelDocRef = new();
    private static readonly DocumentDeserializer<NullableDocument<NamedRef>> _nullabelNamedDocRef = new();

    public NullableDocumentDeserializer()
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
            var v = info.Deserializer.Deserialize(context, ref reader);
            return new NonNullDocument<T>((v as T)!);
        }
        catch (NullDocumentException e)
        {
            return new NullDocument<T>(e.Id, e.Collection, e.Cause);
        }
    }
}
