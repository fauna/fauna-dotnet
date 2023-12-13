using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Fauna.Types;

namespace Fauna.Serialization;

public ref struct Utf8FaunaReader
{
    private Utf8JsonReader _json;
    private readonly Stack<object> _tokenStack = new();
    private TokenType? _bufferedTokenType = null;

    private readonly HashSet<TokenType> _closers = new()
    {
        TokenType.EndObject,
        TokenType.EndSet,
        TokenType.EndDocument,
        TokenType.EndRef,
        TokenType.EndArray
    };

    private string? _taggedTokenValue = null;
    public TokenType CurrentTokenType { get; private set; }

    private enum TokenTypeInternal
    {
        /// <summary>The token type is the start of an escaped Fauna object.</summary>
        StartEscapedObject,
    }

    public Utf8FaunaReader(ReadOnlySequence<byte> bytes)
    {
        _json = new Utf8JsonReader(bytes);
        CurrentTokenType = TokenType.None;
    }

    public Utf8FaunaReader(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        var seq = new ReadOnlySequence<byte>(bytes);
        _json = new Utf8JsonReader(seq);
        CurrentTokenType = TokenType.None;
    }

    public void Skip()
    {
        switch (CurrentTokenType)
        {
            case TokenType.StartObject:
            case TokenType.StartArray:
            case TokenType.StartSet:
            case TokenType.StartRef:
            case TokenType.StartDocument:
                SkipInternal();
                break;
        }
    }

    private void SkipInternal()
    {
        var startCount = _tokenStack.Count;
        while (Read())
        {
            if (_tokenStack.Count < startCount) break;
        }
    }

    public bool Read()
    {
        _taggedTokenValue = null;

        if (_bufferedTokenType != null)
        {
            CurrentTokenType = (TokenType)_bufferedTokenType;
            _bufferedTokenType = null;
            if (_closers.Contains(CurrentTokenType))
            {
                _tokenStack.Pop();
            }
            return true;
        }

        if (!Advance())
        {
            return false;
        }

        switch (_json.TokenType)
        {
            case JsonTokenType.PropertyName:
                CurrentTokenType = TokenType.FieldName;
                break;
            case JsonTokenType.None:
                break;
            case JsonTokenType.StartObject:
                HandleStartObject();
                break;
            case JsonTokenType.EndObject:
                HandleEndObject();
                break;
            case JsonTokenType.StartArray:
                _tokenStack.Push(TokenType.StartArray);
                CurrentTokenType = TokenType.StartArray;
                break;
            case JsonTokenType.EndArray:
                _tokenStack.Pop();
                CurrentTokenType = TokenType.EndArray;
                break;
            case JsonTokenType.String:
                CurrentTokenType = TokenType.String;
                break;
            case JsonTokenType.True:
                CurrentTokenType = TokenType.True;
                break;
            case JsonTokenType.False:
                CurrentTokenType = TokenType.False;
                break;
            case JsonTokenType.Null:
                CurrentTokenType = TokenType.Null;
                break;
            case JsonTokenType.Comment:
            case JsonTokenType.Number:
            default:
                throw new SerializationException($"Unhandled JSON token type {_json.TokenType}.");
        }

        return true;
    }

    public object? GetValue()
    {
        return CurrentTokenType switch
        {
            TokenType.FieldName or TokenType.String => GetString(),
            TokenType.Int => GetInt(),
            TokenType.Long => GetLong(),
            TokenType.Double => GetDouble(),
            TokenType.Date => GetDate(),
            TokenType.Time => GetTime(),
            TokenType.True or TokenType.False => GetBoolean(),
            TokenType.Module => GetModule(),
            _ => throw new SerializationException($"{CurrentTokenType} does not have an associated value")
        };
    }


    public string? GetString()
    {
        if (CurrentTokenType != TokenType.String && CurrentTokenType != TokenType.FieldName)
        {
            throw new InvalidOperationException($"Fauna token value isn't a {TokenType.String.ToString()} or a {TokenType.FieldName.ToString()}.");
        }

        try
        {
            return _json.GetString();
        }
        catch (Exception e)
        {
            throw new SerializationException("Failed to get string", e);
        }
    }

    public bool GetBoolean()
    {
        try
        {
            return _json.GetBoolean();
        }
        catch (Exception e)
        {
            throw new SerializationException("Failed to get boolean", e);
        }
    }

    public DateTime GetDate()
    {
        ValidateTaggedType(TokenType.Date);

        try
        {
            return DateTime.Parse(_taggedTokenValue!);
        }
        catch (Exception e)
        {
            throw new SerializationException($"Failed to get date from {_taggedTokenValue}", e);
        }
    }

    public double GetDouble()
    {
        ValidateTaggedType(TokenType.Double);

        try
        {
            return double.Parse(_taggedTokenValue!, CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            throw new SerializationException($"Failed to get double from {_taggedTokenValue}", e);
        }
    }

    public decimal GetDoubleAsDecimal()
    {
        ValidateTaggedType(TokenType.Double);

        try
        {
            return decimal.Parse(_taggedTokenValue!, CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            throw new SerializationException($"Failed to get decimal from {_taggedTokenValue}", e);
        }
    }

    public int GetInt()
    {
        ValidateTaggedType(TokenType.Int);

        try
        {
            return int.Parse(_taggedTokenValue!);
        }
        catch (Exception e)
        {
            throw new SerializationException($"Failed to get int from {_taggedTokenValue}", e);
        }
    }

    public long GetLong()
    {
        ValidateTaggedType(TokenType.Long);

        try
        {
            return long.Parse(_taggedTokenValue!);
        }
        catch (Exception e)
        {
            throw new SerializationException($"Failed to get long from {_taggedTokenValue}", e);
        }
    }


    public Module GetModule()
    {
        ValidateTaggedType(TokenType.Module);

        return new Module(_taggedTokenValue!);
    }

    public DateTime GetTime()
    {
        ValidateTaggedType(TokenType.Time);

        try
        {
            return DateTime.Parse(_taggedTokenValue!);
        }
        catch (Exception e)
        {
            throw new SerializationException($"Failed to get time from {_taggedTokenValue}", e);
        }
    }

    public string TryGetString(out string value)
    {
        throw new NotImplementedException();
    }

    public bool TryGetBoolean(out bool value)
    {
        throw new NotImplementedException();
    }

    public DateTime TryGetDateTime(out DateTime value)
    {
        throw new NotImplementedException();
    }

    public double TryGetDouble(out double value)
    {
        throw new NotImplementedException();
    }

    public int TryGetInt(out int value)
    {
        throw new NotImplementedException();
    }

    public long TryGetLong(out long value)
    {
        throw new NotImplementedException();
    }

    public Module TryGetModule(out Module value)
    {
        throw new NotImplementedException();
    }

    private void ValidateTaggedType(TokenType type)
    {
        if (CurrentTokenType != type || _taggedTokenValue == null || _taggedTokenValue.GetType() != typeof(string))
        {
            throw new InvalidOperationException($"CurrentTokenType is a {CurrentTokenType.ToString()}, not a {type.ToString()}.");
        }
    }

    private void HandleStartObject()
    {
        AdvanceTrue();

        switch (_json.TokenType)
        {
            case JsonTokenType.PropertyName:
                switch (_json.GetString())
                {
                    case "@date":
                        HandleTaggedString(TokenType.Date);
                        break;
                    case "@doc":
                        AdvanceTrue();
                        CurrentTokenType = TokenType.StartDocument;
                        _tokenStack.Push(TokenType.StartDocument);
                        break;
                    case "@double":
                        HandleTaggedString(TokenType.Double);
                        break;
                    case "@int":
                        HandleTaggedString(TokenType.Int);
                        break;
                    case "@long":
                        HandleTaggedString(TokenType.Long);
                        break;
                    case "@mod":
                        HandleTaggedString(TokenType.Module);
                        break;
                    case "@object":
                        AdvanceTrue();
                        CurrentTokenType = TokenType.StartObject;
                        _tokenStack.Push(TokenTypeInternal.StartEscapedObject);
                        break;
                    case "@ref":
                        AdvanceTrue();
                        CurrentTokenType = TokenType.StartRef;
                        _tokenStack.Push(TokenType.StartRef);
                        break;
                    case "@set":
                        AdvanceTrue();
                        CurrentTokenType = TokenType.StartSet;
                        _tokenStack.Push(TokenType.StartSet);
                        break;
                    case "@time":
                        HandleTaggedString(TokenType.Time);
                        break;
                    default:
                        _bufferedTokenType = TokenType.FieldName;
                        _tokenStack.Push(TokenType.StartObject);
                        CurrentTokenType = TokenType.StartObject;
                        break;
                }
                break;
            case JsonTokenType.EndObject:
                _bufferedTokenType = TokenType.EndObject;
                _tokenStack.Push(TokenType.StartObject);
                CurrentTokenType = TokenType.StartObject;
                break;
            default:
                throw new SerializationException($"Unexpected token following StartObject: {_json.TokenType}");
        }
    }

    private void HandleEndObject()
    {
        var startToken = _tokenStack.Pop();
        switch (startToken)
        {
            case TokenType.StartDocument:
                CurrentTokenType = TokenType.EndDocument;
                AdvanceTrue();
                break;
            case TokenType.StartSet:
                CurrentTokenType = TokenType.EndSet;
                AdvanceTrue();
                break;
            case TokenType.StartRef:
                CurrentTokenType = TokenType.EndRef;
                AdvanceTrue();
                break;
            case TokenTypeInternal.StartEscapedObject:
                CurrentTokenType = TokenType.EndObject;
                AdvanceTrue();
                break;
            case TokenType.StartObject:
                CurrentTokenType = TokenType.EndObject;
                break;
            default:
                throw new SerializationException($"Unexpected token {startToken}. This might be a bug.");
        }
    }

    /// <summary>
    /// Method <c>HandleTaggedString</c> is used to advance through a JSON object that represents a tagged type with a
    /// a string value. For example:
    /// 
    /// * Given { "@int": "123" }
    /// * Read JSON until JsonTokenType.PropertyName and you've determined it's an int
    /// * Call HandleTaggedString(TokenType.Int)
    /// * The underlying JSON reader is advanced until JsonTokenType.EndObject
    /// * Access the int via GetInt()
    /// 
    /// </summary>
    private void HandleTaggedString(TokenType token)
    {
        AdvanceTrue();
        CurrentTokenType = token;
        _taggedTokenValue = _json.GetString();
        AdvanceTrue();
    }

    private bool Advance()
    {
        try
        {
            return _json.Read();
        }
        catch (Exception e)
        {
            throw new SerializationException("Failed to advance underlying JSON reader.", e);
        }
    }

    private void AdvanceTrue()
    {
        if (!Advance())
        {
            throw new SerializationException("Unexpected end of underlying JSON reader.");
        }
    }
}