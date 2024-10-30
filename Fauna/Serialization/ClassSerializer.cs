using System.Diagnostics;
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization;

internal class ClassSerializer<T> : BaseSerializer<T>, IPartialDocumentSerializer
{
    private const string IdField = "id";
    private const string NameField = "name";
    private const string CollField = "coll";
    private readonly MappingInfo _info;

    public ClassSerializer(MappingInfo info)
    {
        Debug.Assert(info.Type == typeof(T));
        _info = info;
    }

    public override List<FaunaType> GetSupportedTypes() => new List<FaunaType> { FaunaType.Document, FaunaType.Null, FaunaType.Object, FaunaType.Ref };

    public object DeserializeDocument(MappingContext context, string? id, string? name, Module? coll, ref Utf8FaunaReader reader)
    {
        object instance = CreateInstance();
        if (id is not null) TrySetId(instance, id);
        if (name is not null) TrySetName(instance, name);
        if (coll is not null) TrySetColl(instance, coll);
        SetFields(instance, context, ref reader, TokenType.EndDocument);
        return instance;
    }

    public override T Deserialize(MappingContext ctx, ref Utf8FaunaReader reader)
    {
        var endToken = reader.CurrentTokenType switch
        {
            TokenType.StartDocument => TokenType.EndDocument,
            TokenType.StartObject => TokenType.EndObject,
            TokenType.StartRef => TokenType.EndRef,
            var other => throw UnexpectedToken(other),
        };

        if (endToken == TokenType.EndRef)
        {
            string? id = null;
            string? name = null;
            Module? coll = null;
            string? cause = null;
            bool exists = true;

            while (reader.Read() && reader.CurrentTokenType != TokenType.EndRef)
            {
                if (reader.CurrentTokenType != TokenType.FieldName)
                    throw new SerializationException(
                        $"Unexpected token while deserializing into Ref: {reader.CurrentTokenType}");

                string fieldName = reader.GetString()!;
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
                throw new SerializationException("Cannot deserialize refs into classes.");
            }

            if (id != null)
            {
                throw new NullDocumentException(id, null, coll!, cause!);
            }

            throw new NullDocumentException(null, name, coll!, cause!);
        }

        object instance = CreateInstance();
        SetFields(instance, ctx, ref reader, endToken);
        return (T)instance;
    }

    public override void Serialize(MappingContext ctx, Utf8FaunaWriter writer, object? o)
    {
        SerializeInternal(ctx, writer, o);
    }

    private static void SerializeInternal(MappingContext ctx, Utf8FaunaWriter w, object? o)
    {
        if (o == null)
        {
            w.WriteNullValue();
            return;
        }

        var t = o.GetType();
        var info = ctx.GetInfo(t);
        bool shouldEscape = info.ShouldEscapeObject;

        if (shouldEscape) w.WriteStartEscapedObject(); else w.WriteStartObject();
        foreach (var field in info.Fields)
        {
            if (field.FieldType is FieldType.ServerGeneratedId or FieldType.Ts or FieldType.Coll)
            {
                continue;
            }

            object? v = field.Property.GetValue(o);
            if (field.FieldType is FieldType.ClientGeneratedId && v == null)
            {
                // The field is a client generated ID but set to null, so assume they're doing something
                // other than creating the object.
                continue;
            }

            w.WriteFieldName(field.Name);
            field.Serializer.Serialize(ctx, w, v);
        }
        if (shouldEscape) w.WriteEndEscapedObject(); else w.WriteEndObject();
    }

    private object CreateInstance() => Activator.CreateInstance(_info.Type)!;

    private void SetFields(object instance, MappingContext context, ref Utf8FaunaReader reader, TokenType endToken)
    {
        while (reader.Read() && reader.CurrentTokenType != endToken)
        {
            if (reader.CurrentTokenType != TokenType.FieldName)
                throw UnexpectedToken(reader.CurrentTokenType);

            string fieldName = reader.GetString()!;
            reader.Read();

            if (fieldName == IdField && reader.CurrentTokenType == TokenType.String)
            {
                TrySetId(instance, reader.GetString()!);
            }
            else if (fieldName == NameField && reader.CurrentTokenType == TokenType.String)
            {
                TrySetName(instance, reader.GetString()!);
            }
            else if (_info.FieldsByName.TryGetValue(fieldName, out var field))
            {
                field.Property.SetValue(instance, field.Serializer.Deserialize(context, ref reader));
            }
            else
            {
                reader.Skip();
            }
        }
    }

    private void TrySetId(object instance, string id)
    {
        if (!_info.FieldsByName.TryGetValue(IdField, out var field))
        {
            return;
        }

        if (field.Type == typeof(long))
        {
            field.Property.SetValue(instance, long.Parse(id));
        }
        else if (field.Type == typeof(string))
        {
            field.Property.SetValue(instance, id);
        }
        else
        {
            throw UnexpectedToken(TokenType.String);
        }
    }

    private void TrySetName(object instance, string name)
    {
        if (_info.FieldsByName.TryGetValue(NameField, out var field))
        {
            if (field.Type == typeof(string))
            {
                field.Property.SetValue(instance, name);
            }
            else
            {
                throw UnexpectedToken(TokenType.String);
            }
        }
    }

    private void TrySetColl(object instance, Module coll)
    {
        if (_info.FieldsByName.TryGetValue(CollField, out var field))
        {
            if (field.Type == typeof(Module))
            {
                field.Property.SetValue(instance, coll);
            }
            else
            {
                throw UnexpectedToken(TokenType.String);
            }
        }
    }

    private new SerializationException UnexpectedToken(TokenType tokenType) =>
        new($"Unexpected token while deserializing into class {_info.Type.Name}: {tokenType}");
}
