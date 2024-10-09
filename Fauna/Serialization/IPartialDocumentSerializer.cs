using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal interface IPartialDocumentSerializer : ISerializer
{
    public object DeserializeDocument(MappingContext context, string? id, string? name, Module? coll, ref Utf8FaunaReader reader);
}
