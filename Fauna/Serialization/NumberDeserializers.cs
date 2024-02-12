using Fauna.Mapping;

namespace Fauna.Serialization;

internal class LongDeserializer : BaseDeserializer<long>
{
    public override long Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Int => reader.GetInt(),
            TokenType.Long => reader.GetLong(),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing: {reader.CurrentTokenType}"),
        };
}
