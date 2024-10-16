using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;


internal class ModuleSerializer : BaseSerializer<Module>
{
    public override List<FaunaType> GetSupportedTypes() => new List<FaunaType> { FaunaType.Module, FaunaType.Null };

    public override Module Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        reader.CurrentTokenType switch
        {
            TokenType.Module => reader.GetModule(),
            _ => throw UnexpectedToken(reader.CurrentTokenType)
        };

    public override void Serialize(MappingContext context, Utf8FaunaWriter writer, object? o)
    {
        switch (o)
        {
            case null:
                writer.WriteNullValue();
                break;
            case Module module:
                writer.WriteModuleValue(module);
                break;
            default:
                throw new SerializationException(UnsupportedSerializationTypeMessage(o.GetType()));
        }
    }
}
