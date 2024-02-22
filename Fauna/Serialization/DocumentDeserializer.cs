using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal class DocumentDeserializer<T> : BaseDeserializer<T>
{
    public override T Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        return reader.CurrentTokenType switch
        {
            TokenType.StartObject => DeserializeDocument(context, ref reader),
            TokenType.StartRef => DeserializeRef(context, ref reader),
            _ => throw new SerializationException(
                $"Unexpected token while deserializing into {typeof(NullableDocument<T>)}: {reader.CurrentTokenType}")
        };
    }

    private T DeserializeDocument(MappingContext context, ref Utf8FaunaReader reader)
    {
        var data = new Dictionary<string, object?>();
        string? id = null;
        object? name = null;
        DateTime? ts = null;
        Module? coll = null;

        while (reader.Read() && reader.CurrentTokenType != TokenType.EndDocument)
        {
            if (reader.CurrentTokenType != TokenType.FieldName)
                throw new SerializationException(
                    $"Unexpected token while deserializing into Document: {reader.CurrentTokenType}");

            var fieldName = reader.GetString()!;
            reader.Read();
            switch (fieldName)
            {
                case "id":
                    id = reader.GetString();
                    break;
                case "name":
                    name = DynamicDeserializer.Singleton.Deserialize(context, ref reader);
                    break;
                case "coll":
                    coll = reader.GetModule();

                    // if we encounter a mapped collection, jump to the class deserializer.
                    // NB this relies on the fact that docs on the wire always
                    // start with id and coll.
                    if (context.TryGetCollection(coll.Name, out var collInfo))
                    {
                        // This assumes ordering on the wire. If name is not null and we're here, then it's a named document so name is a string.
                        var doc = collInfo.Deserializer.DeserializeDocument(context, id, name != null ? (string)name : null, ref reader);
                        if (doc is T v) return v;
                        throw new SerializationException($"Expected type {typeof(T)} but received {doc.GetType()}");
                    }

                    break;
                case "ts":
                    ts = reader.GetTime();
                    break;
                default:
                    data[fieldName] = DynamicDeserializer.Singleton.Deserialize(context, ref reader);
                    break;
            }
        }

        if (id != null && coll != null && ts != null)
        {
            if (name != null) data["name"] = name;
            var r = new Document(id, coll, ts.GetValueOrDefault(), data);
            if (r is T d) return d;
            var nr = (NullableDocument<Document>)new NonNullDocument<Document>(r);
            if (nr is T nnd) return nnd;
        }

        if (name != null && coll != null && ts != null)
        {
            // If we're here, name is a string.
            var r = new NamedDocument((string)name, coll, ts.GetValueOrDefault(), data);
            if (r is T d) return d;
            var nr = (NullableDocument<NamedDocument>)new NonNullDocument<NamedDocument>(r);
            if (nr is T nnd) return nnd;
        }

        throw new SerializationException("Unsupported document type.");
    }

    private T DeserializeRef(MappingContext context, ref Utf8FaunaReader reader)
    {
        string? id = null;
        string? name = null;
        Module? coll = null;
        string? cause = null;
        var exists = true;

        while (reader.Read() && reader.CurrentTokenType != TokenType.EndRef)
        {
            if (reader.CurrentTokenType != TokenType.FieldName)
                throw new SerializationException(
                    $"Unexpected token while deserializing into DocumentRef: {reader.CurrentTokenType}");

            var fieldName = reader.GetString()!;
            reader.Read();
            switch (fieldName)
            {
                case "id":
                    id = reader.GetString();
                    break;
                case "name":
                    name = reader.GetString();
                    break;
                case "coll":
                    coll = reader.GetModule();
                    break;
                case "cause":
                    cause = reader.GetString();
                    break;
                case "exists":
                    exists = reader.GetBoolean();
                    break;
            }
        }

        if ((id != null || name != null) && coll != null && exists)
        {
            throw new SerializationException($"Expected a document but received a ref: {id ?? name} in {coll.Name}");
        }

        if ((id != null || name != null) && coll != null && !exists)
        {
            var ty = typeof(T);
            if (ty.IsGenericType && ty.GetGenericTypeDefinition() == typeof(NullableDocument<>))
            {
                var inner = ty.GetGenericArguments()[0];
                var nty = typeof(NullDocument<>).MakeGenericType(inner);
                var r = Activator.CreateInstance(nty, (id ?? name)!, coll, cause!);
                return (T)r!;
            }
        }

        throw new SerializationException("Unsupported reference type");
    }

}
