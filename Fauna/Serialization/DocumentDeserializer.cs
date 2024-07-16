using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal class DocumentDeserializer<T> : BaseDeserializer<T> where T : class
{

    public override T Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        return reader.CurrentTokenType switch
        {
            TokenType.StartDocument => DeserializeDocument(context, ref reader),
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
                        // If the user asks for a different type, don't use the saved deserializer.
                        if (collInfo.Type == typeof(T) || typeof(object) == typeof(T))
                        {
                            // This assumes ordering on the wire. If name is not null and we're here, then it's a named document so name is a string.
                            return (collInfo.Deserializer.DeserializeDocument(context, id, name != null ? (string)name : null, ref reader) as T)!;
                        }
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
            // For convenience, if a user asks for a DocumentRef but gets a document, give them the ref.
            if (typeof(Ref) == typeof(T))
            {
                return (new Ref(id, coll) as T)!;
            }

            // For convenience, if a user asks for a NullableDocument<DocumentRef> but gets a document, give it to them.
            if (typeof(NullableDocument<Ref>) == typeof(T))
            {
                var docRef = new Ref(id, coll);
                return (new NonNullDocument<Ref>(docRef) as T)!;
            }

            if (name != null) data["name"] = name;

            var doc = new Document(id, coll, ts.GetValueOrDefault(), data);
            if (typeof(Document) == typeof(T))
            {
                return (doc as T)!;
            }

            return (new NonNullDocument<Document>(doc) as T)!;
        }

        if (name != null && coll != null && ts != null)
        {
            // If we're here, name is a string.
            var nameAsString = (string)name;
            var r = new NamedDocument(nameAsString, coll, ts.GetValueOrDefault(), data);
            if (r is T d) return d;
            var nr = (NullableDocument<NamedDocument>)new NonNullDocument<NamedDocument>(r);
            if (nr is T nnd) return nnd;

            // For convenience, if a user asks for a NamedDocumentRef but gets a named document, give them the ref.
            if (typeof(NamedRef) == typeof(T))
            {
                return (new NamedRef(nameAsString, coll) as T)!;
            }

            // For convenience, if a user asks for a NullableDocument<NamedDocumentRef> but gets a named document, give it to them.
            if (typeof(NullableDocument<NamedRef>) == typeof(T))
            {
                var docRef = new NamedRef(nameAsString, coll);
                return (new NonNullDocument<NamedRef>(docRef) as T)!;
            }

            var doc = new NamedDocument(nameAsString, coll, ts.GetValueOrDefault(), data);
            if (typeof(NamedDocument) == typeof(T))
            {
                return (doc as T)!;
            }

            return (new NonNullDocument<NamedDocument>(doc) as T)!;
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

        if (id != null && coll != null && exists)
        {
            var docRef = new Ref(id, coll);
            if (typeof(NullableDocument<Ref>) == typeof(T))
            {
                return (new NonNullDocument<Ref>(docRef) as T)!;
            }

            return (docRef as T)!;
        }

        if (name != null && coll != null && exists)
        {
            var docRef = new NamedRef(name, coll);
            if (typeof(NullableDocument<NamedRef>) == typeof(T))
            {
                return (new NonNullDocument<NamedRef>(docRef) as T)!;
            }

            return (docRef as T)!;
        }

        if (id != null && coll != null && !exists)
        {
            if (typeof(Ref) == typeof(T) || typeof(Document) == typeof(T))
            {
                throw new NullDocumentException(id, coll, cause!);
            }

            if (typeof(NullableDocument<Ref>) == typeof(T))
            {
                return (new NullDocument<Ref>(id, coll, cause!) as T)!;
            }

            return (new NullDocument<Document>(id, coll, cause!) as T)!;
        }

        if (name != null && coll != null && !exists)
        {
            if (typeof(NamedRef) == typeof(T) || typeof(NamedDocument) == typeof(T))
            {
                throw new NullDocumentException(name, coll, cause!);
            }

            if (typeof(NullableDocument<NamedRef>) == typeof(T))
            {
                return (new NullDocument<NamedRef>(name, coll, cause!) as T)!;
            }

            return (new NullDocument<NamedDocument>(name, coll, cause!) as T)!;
        }


        throw new SerializationException("Unsupported reference type");
    }

}
