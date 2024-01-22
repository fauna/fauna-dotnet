using Fauna.Mapping;
using System.Diagnostics;

namespace Fauna.Serialization;

internal class ClassDeserializer<T> : BaseDeserializer<T>
{
    private readonly MappingInfo _info;

    public ClassDeserializer(MappingInfo info)
    {
        Debug.Assert(info.Type == typeof(T));
        _info = info;
    }

    public override T Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        var endToken = reader.CurrentTokenType switch
        {
            TokenType.StartDocument => TokenType.EndDocument,
            TokenType.StartObject => TokenType.EndObject,
            var other => throw UnexpectedToken(other),
        };

        var instance = Activator.CreateInstance(_info.Type)!;

        while (reader.Read() && reader.CurrentTokenType != endToken)
        {
            if (reader.CurrentTokenType != TokenType.FieldName)
                throw UnexpectedToken(reader.CurrentTokenType);

            var fieldName = reader.GetString()!;
            reader.Read();

            if (_info.FieldsByName.TryGetValue(fieldName, out var field))
            {
                var deser = Deserializer.Generate(context, field.Type);
                if (field.IsNullable)
                {
                    deser = new NullableDeserializer(deser);
                }
                field.Property.SetValue(instance, deser.Deserialize(context, ref reader));
            }
            else
            {
                reader.Skip();
            }
        }

        return (T)instance;
    }

    private SerializationException UnexpectedToken(TokenType tokenType) =>
        new SerializationException(
            $"Unexpected token while deserializing into class {_info.Type.Name}: {tokenType}");
}
