using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;
using Fauna.Types;

namespace Fauna.Serialization;

/// <summary>
/// Represents a reader that provides fast, non-cached, forward-only access to serialized data.
/// </summary>
public ref struct Utf8FaunaReader
{
    private Utf8JsonReader _json;
    private readonly Stack<object> _tokenStack = new();
    private TokenType? _bufferedTokenType = null;

    private readonly HashSet<TokenType> _closers = new()
    {
        TokenType.EndObject,
        TokenType.EndPage,
        TokenType.EndDocument,
        TokenType.EndRef,
        TokenType.EndArray
    };

    private string? _taggedTokenValue = null;

    /// <summary>
    /// Gets the type of the current token.
    /// </summary>
    public TokenType CurrentTokenType { get; private set; }

    private enum TokenTypeInternal
    {
        /// <summary>The token type is the start of an escaped Fauna object.</summary>
        StartEscapedObject,
    }

    /// <summary>
    /// Initializes a new Utf8FaunaReader to read from a ReadOnlySequence of bytes.
    /// </summary>
    /// <param name="bytes">The sequence of bytes to read from.</param>
    public Utf8FaunaReader(ReadOnlySequence<byte> bytes)
    {
        _json = new Utf8JsonReader(bytes);
        CurrentTokenType = TokenType.None;
    }

    /// <summary>
    /// Initializes a new Utf8FaunaReader to read from a string.
    /// </summary>
    /// <param name="str">The string to read from.</param>
    public Utf8FaunaReader(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        var seq = new ReadOnlySequence<byte>(bytes);
        _json = new Utf8JsonReader(seq);
        CurrentTokenType = TokenType.None;
    }

    /// <summary>
    /// Skips the value of the current token.
    /// </summary>
    public void Skip()
    {
        switch (CurrentTokenType)
        {
            case TokenType.StartObject:
            case TokenType.StartArray:
            case TokenType.StartPage:
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

    /// <summary>
    /// Reads the next token from the source.
    /// </summary>
    /// <returns>true if the token was read successfully; otherwise, false.</returns>
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

    /// <summary>
    /// Gets the value of the current token.
    /// </summary>
    /// <returns>The value of the current token, or null if no value is associated with the token.</returns>
    /// <exception cref="SerializationException">Thrown when an error occurs during token value retrieval.</exception>
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

    /// <summary>
    /// Retrieves a string value from the current token.
    /// </summary>
    /// <returns>A string representation of the current token's value.</returns>
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

    /// <summary>
    /// Retrieves a boolean value from the current JSON token.
    /// </summary>
    /// <returns>A boolean representation of the current token's value.</returns>
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

    /// <summary>
    /// Retrieves a DateOnly value from the current token.
    /// </summary>
    /// <returns>A DateOnly representation of the current token's value.</returns>
    public DateOnly GetDate()
    {
        ValidateTaggedType(TokenType.Date);

        try
        {
            return DateOnly.Parse(_taggedTokenValue!);
        }
        catch (Exception e)
        {
            throw new SerializationException($"Failed to get date from {_taggedTokenValue}", e);
        }
    }

    /// <summary>
    /// Retrieves a double value from the current token.
    /// </summary>
    /// <returns>A double representation of the current token's value.</returns>
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

    /// <summary>
    /// Retrieves a decimal value from the current token.
    /// </summary>
    /// <returns>A decimal representation of the current token's value.</returns>
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

    /// <summary>
    /// Retrieves an integer value from the current token.
    /// </summary>
    /// <returns>An integer representation of the current token's value.</returns>
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

    /// <summary>
    /// Retrieves an short value from the current token.
    /// </summary>
    /// <returns>An short representation of the current token's value.</returns>
    public short GetShort()
    {
        ValidateTaggedTypes(TokenType.Int, TokenType.Long);
        try
        {
            return short.Parse(_taggedTokenValue!);
        }
        catch (Exception e)
        {
            throw new SerializationException($"Failed to get short from {_taggedTokenValue}", e);
        }
    }

    /// <summary>
    /// Retrieves an unsigned short value from the current token.
    /// </summary>
    /// <returns>An unsigned short representation of the current token's value.</returns>
    public ushort GetUnsignedShort()
    {
        ValidateTaggedTypes(TokenType.Int, TokenType.Long);
        try
        {
            return ushort.Parse(_taggedTokenValue!);
        }
        catch (Exception e)
        {
            throw new SerializationException($"Failed to get ushort from {_taggedTokenValue}", e);
        }
    }

    /// <summary>
    /// Retrieves a long value from the current token.
    /// </summary>
    /// <returns>A long representation of the current token's value.</returns>
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

    /// <summary>
    /// Retrieves a Module object from the current token.
    /// </summary>
    /// <returns>A Module representation of the current token's value.</returns>
    public Module GetModule()
    {
        ValidateTaggedType(TokenType.Module);

        return new Module(_taggedTokenValue!);
    }

    /// <summary>
    /// Retrieves a DateTime value from the current token.
    /// </summary>
    /// <returns>A DateTime representation of the current token's value.</returns>
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

    /// <summary>
    /// Tries to retrieve a string value from the current token.
    /// </summary>
    /// <param name="value">When this method returns, contains the string value, if the conversion succeeded, or null if the conversion failed.</param>
    /// <returns>true if the token's value could be converted to a string; otherwise, false.</returns>
    public string TryGetString(out string value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Tries to retrieve a boolean value from the current token.
    /// </summary>
    /// <param name="value">When this method returns, contains the boolean value, if the conversion succeeded, or false if the conversion failed.</param>
    /// <returns>true if the token's value could be converted to a boolean; otherwise, false.</returns>
    public bool TryGetBoolean(out bool value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Tries to retrieve a DateTime value from the current token.
    /// </summary>
    /// <param name="value">When this method returns, contains the DateTime value, if the conversion succeeded, or the default DateTime value if the conversion failed.</param>
    /// <returns>true if the token's value could be converted to a DateTime; otherwise, false.</returns>
    public DateTime TryGetDateTime(out DateTime value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Tries to retrieve a double value from the current token.
    /// </summary>
    /// <param name="value">When this method returns, contains the double value, if the conversion succeeded, or 0.0 if the conversion failed.</param>
    /// <returns>true if the token's value could be converted to a double; otherwise, false.</returns>
    public double TryGetDouble(out double value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Tries to retrieve an integer value from the current token.
    /// </summary>
    /// <param name="value">When this method returns, contains the integer value, if the conversion succeeded, or 0 if the conversion failed.</param>
    /// <returns>true if the token's value could be converted to an integer; otherwise, false.</returns>
    public int TryGetInt(out int value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Tries to retrieve a long value from the current token.
    /// </summary>
    /// <param name="value">When this method returns, contains the long value, if the conversion succeeded, or 0 if the conversion failed.</param>
    /// <returns>true if the token's value could be converted to a long; otherwise, false.</returns>
    public long TryGetLong(out long value)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Tries to retrieve a Module object from the current token.
    /// </summary>
    /// <param name="value">When this method returns, contains the Module object, if the conversion succeeded, or null if the conversion failed.</param>
    /// <returns>true if the token's value could be converted to a Module; otherwise, false.</returns>
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

    private void ValidateTaggedTypes(params TokenType[] types)
    {
        if (!types.Contains(CurrentTokenType) || _taggedTokenValue == null || _taggedTokenValue.GetType() != typeof(string))
        {
            throw new InvalidOperationException($"CurrentTokenType is a {CurrentTokenType.ToString()}, not in {types}.");
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
                        CurrentTokenType = TokenType.StartPage;
                        _tokenStack.Push(TokenType.StartPage);
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
            case TokenType.StartPage:
                CurrentTokenType = TokenType.EndPage;
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