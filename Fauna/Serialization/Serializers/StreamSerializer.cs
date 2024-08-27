using Fauna.Mapping;
using Stream = Fauna.Types.Stream;

namespace Fauna.Serialization.Serializers;


internal class StreamSerializer : BaseSerializer<Stream>
{
    public override Stream Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Stream => reader.GetStream(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        throw new NotImplementedException();
    }
}
