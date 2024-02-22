using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal class NullableDocumentDeserializer<T> : BaseDeserializer<NullableDocument<T>>
{
    private readonly IDeserializer<NullableDocument<T>> _valueDeserializer;

    public NullableDocumentDeserializer(IDeserializer<NullableDocument<T>> valueDeserializer)
    {
        _valueDeserializer = valueDeserializer;
    }

    public override NullableDocument<T> Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        if (reader.CurrentTokenType is not (TokenType.StartObject or TokenType.StartRef))
            throw new SerializationException(
                $"Unexpected token while deserializing into {typeof(NullableDocument<T>)}: {reader.CurrentTokenType}");

        return _valueDeserializer.Deserialize(context, ref reader);
    }
}
