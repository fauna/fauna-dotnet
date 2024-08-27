using System.Diagnostics;
using Fauna.Exceptions;
using Fauna.Mapping;
using Fauna.Types;

namespace Fauna.Serialization.Serializers;

internal interface IClassDocumentSerializer : ISerializer
{
    public object DeserializeDocument(MappingContext context, string? id, string? name, ref Utf8FaunaReader reader);
}

internal class ClassSerializer<T> : BaseSerializer<T>, IClassDocumentSerializer
{
    private const string IdField = "id";
    private const string NameField = "name";
    private readonly MappingInfo _info;
    private readonly bool _isDocument;

    public ClassSerializer(MappingInfo info)
    {
        Debug.Assert(info.Type == typeof(T));
        _info = info;
    }

    public ClassSerializer(MappingInfo info, bool isDocument)
    {
        Debug.Assert(info.Type == typeof(T));
        _info = info;
        _isDocument = isDocument;
    }

    public object DeserializeDocument(MappingContext context, string? id, string? name, ref Utf8FaunaReader reader)
    {
        object instance = CreateInstance();
        if (id is not null) TrySetId(instance, id);
        if (name is not null) TrySetName(instance, name);
        SetFields(instance, context, ref reader, TokenType.EndDocument);
        return instance;
    }

    public override T Deserialize(MappingContext context, ref Utf8FaunaReader reader)
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
                        $"Unexpected token while deserializing into DocumentRef: {reader.CurrentTokenType}");

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

            throw new NullDocumentException((id ?? name)!, coll!, cause!);
        }

        object instance = CreateInstance();
        SetFields(instance, context, ref reader, endToken);
        return (T)instance;
    }

    public override void Serialize(MappingContext ctx, Utf8FaunaWriter w, object? o)
    {
        var skipFields = new HashSet<string>();

        if (_isDocument)
        {
            foreach (FieldInfo fi in _info.Fields)
            {
                if (fi.Name.ToLowerInvariant() == "id" && fi.Property.GetValue(o) is null) skipFields.Add("id");
                if (fi.Name.ToLowerInvariant() == "coll" && fi.Property.GetValue(o) is null) skipFields.Add("coll");
                if (fi.Name.ToLowerInvariant() == "ts" && fi.Property.GetValue(o) is null) skipFields.Add("ts");
            }
        }

        SerializeInternal(ctx, w, o, skipFields);
    }

    private static void SerializeInternal(MappingContext ctx, Utf8FaunaWriter w, object? o, IReadOnlySet<string> skipFields)
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
            if (skipFields.Contains(field.Name.ToLowerInvariant())) continue;

            w.WriteFieldName(field.Name);
            object? v = field.Property.GetValue(o);
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

    private new SerializationException UnexpectedToken(TokenType tokenType) =>
        new($"Unexpected token while deserializing into class {_info.Type.Name}: {tokenType}");
}
