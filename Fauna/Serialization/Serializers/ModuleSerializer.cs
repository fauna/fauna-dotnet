using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;


internal class ModuleSerializer : BaseSerializer<Module>
{
    public override Module Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Module => reader.GetModule(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        DynamicSerializer.Singleton.Serialize(context, writer, o);
    }
}
