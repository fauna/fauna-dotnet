namespace Fauna.Serialization;

public class LinqProjectionDeserializer<T> : BaseDeserializer<T>
{
    private IDeserializer[] _fieldDeserializers;
    private Delegate _mapper;

    public LinqProjectionDeserializer(IDeserializer[] fields, Delegate mapper)
    {
        _fieldDeserializers = fields;
        _mapper = mapper;
    }

    public override T Deserialize(SerializationContext context, ref Utf8FaunaReader reader)
    {
        if (reader.CurrentTokenType != TokenType.StartArray)
            throw UnexpectedToken(reader.CurrentTokenType);

        var fields = new object?[_fieldDeserializers.Length];

        for (var i = 0; i < _fieldDeserializers.Length; i++)
        {
            if (!reader.Read()) throw new SerializationException("Unexpected end of stream");
            if (reader.CurrentTokenType == TokenType.EndArray) throw UnexpectedToken(reader.CurrentTokenType);

            fields[i] = _fieldDeserializers[i].Deserialize(context, ref reader);
        }

        if (!reader.Read()) throw new SerializationException("Unexpected end of stream");
        if (reader.CurrentTokenType != TokenType.EndArray) throw UnexpectedToken(reader.CurrentTokenType);

        return (T)_mapper.DynamicInvoke(fields)!;
    }

    private SerializationException UnexpectedToken(TokenType tokenType) =>
        new SerializationException($"Unexpected token while deserializing LINQ element: {tokenType}");
}

public class LinqDocumentDeserializer<T> : BaseDeserializer<T>
{
    private IDeserializer _docDeserializer;
    private Delegate _mapper;

    public LinqDocumentDeserializer(IDeserializer doc, Delegate mapper)
    {
        _docDeserializer = doc;
        _mapper = mapper;
    }

    public override T Deserialize(SerializationContext context, ref Utf8FaunaReader reader)
    {
        var doc = _docDeserializer.Deserialize(context, ref reader);
        return (T)_mapper.DynamicInvoke(new[] { doc })!;
    }
}
