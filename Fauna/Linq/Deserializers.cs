using Fauna.Mapping;
using Fauna.Serialization;

namespace Fauna.Linq;

internal class MappedDeserializer<I, O> : BaseDeserializer<O>
{
    private IDeserializer<I> _inner;
    private Func<I, O> _mapper;

    public MappedDeserializer(IDeserializer<I> inner, Func<I, O> mapper)
    {
        _inner = inner;
        _mapper = mapper;
    }

    public override O Deserialize(MappingContext context, ref Utf8FaunaReader reader) =>
        _mapper(_inner.Deserialize(context, ref reader));
}

internal class ProjectionDeserializer : BaseDeserializer<object?[]>
{
    private IDeserializer[] _fields;

    public ProjectionDeserializer(IEnumerable<IDeserializer> fields)
    {
        _fields = fields.ToArray();
    }

    public override object?[] Deserialize(MappingContext context, ref Utf8FaunaReader reader)
    {
        if (reader.CurrentTokenType != TokenType.StartArray)
            throw UnexpectedToken(reader.CurrentTokenType);

        var values = new object?[_fields.Length];

        for (var i = 0; i < _fields.Length; i++)
        {
            if (!reader.Read()) throw new SerializationException("Unexpected end of stream");
            if (reader.CurrentTokenType == TokenType.EndArray) throw UnexpectedToken(reader.CurrentTokenType);

            values[i] = _fields[i].Deserialize(context, ref reader);
        }

        if (!reader.Read()) throw new SerializationException("Unexpected end of stream");
        if (reader.CurrentTokenType != TokenType.EndArray) throw UnexpectedToken(reader.CurrentTokenType);

        return values;
    }

    private new static SerializationException UnexpectedToken(TokenType tokenType) =>
        new($"Unexpected token while deserializing LINQ element: {tokenType}");
}
