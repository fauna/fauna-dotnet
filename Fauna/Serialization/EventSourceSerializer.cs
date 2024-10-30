using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;


internal class EventSourceSerializer : BaseSerializer<EventSource>
{
    public override List<FaunaType> GetSupportedTypes() => new List<FaunaType> { FaunaType.Null, FaunaType.Stream };

    public override EventSource Deserialize(MappingContext ctx, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.EventSource => reader.GetEventSource(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        throw new NotImplementedException();
    }
}
