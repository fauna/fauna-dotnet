using Fauna.Serialization.Attributes;

namespace Fauna.Serialization;

internal class ClassDeserializer<T> : BaseDeserializer<T>
{
    private Dictionary<string, FieldAttribute> _fieldMap;
    private Type _targetType;

    public ClassDeserializer(Dictionary<string, FieldAttribute> fieldMap)
    {
        _fieldMap = fieldMap;
        _targetType = typeof(T);
    }

    public override T Deserialize(SerializationContext context, ref Utf8FaunaReader reader)
    {
        var endToken = reader.CurrentTokenType switch
        {
            TokenType.StartDocument => TokenType.EndDocument,
            TokenType.StartObject => TokenType.EndObject,
            var other => throw UnexpectedToken(other),
        };

        var instance = Activator.CreateInstance(_targetType);

        while (reader.Read() && reader.CurrentTokenType != endToken)
        {
            if (reader.CurrentTokenType == TokenType.FieldName)
            {
                var fieldName = reader.GetString()!;
                reader.Read();

                if (_fieldMap.ContainsKey(fieldName))
                {
                    _fieldMap[fieldName].Info!.SetValue(
                        instance,
                        DynamicDeserializer.Singleton.Deserialize(context, ref reader)!);
                }
                else
                {
                    reader.Skip();
                }
            }
            else
            {
                throw UnexpectedToken(reader.CurrentTokenType);
            }
        }

        return (T)instance!;
    }

    private SerializationException UnexpectedToken(TokenType tokenType) =>
            new SerializationException($"Unexpected token while deserializing into class {_targetType.Name}: {tokenType}");
}
