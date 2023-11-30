using System.Buffers;
using System.Globalization;
using System.Text.Json;
using Fauna.Types;

namespace Fauna.Serialization;

public ref struct Utf8FaunaReader
{
    private Utf8JsonReader _json;
    private readonly Stack<TokenType> _tokenStack = new();
    private bool _bufferedStartObject = false;

    private string? _taggedTokenValue = null;
    public TokenType CurrentTokenType { get; private set; }
    
    public Utf8FaunaReader(ReadOnlySequence<byte> bytes)
    {
        _json = new Utf8JsonReader(bytes);
        CurrentTokenType = TokenType.None;
    }

    public bool Read()
    {
        if (_bufferedStartObject)
        {
            _bufferedStartObject = false;
            CurrentTokenType = TokenType.FieldName;
            return true;
        }
            
        if (!Advance())
        {
            return false;
        }
            
        switch (_json.TokenType)
        {
            case JsonTokenType.PropertyName:
            {
                CurrentTokenType = TokenType.FieldName;
                break;
            }
            case JsonTokenType.Number:
            {
                throw new NotImplementedException();
            }
            case JsonTokenType.None:
                break;
            case JsonTokenType.StartObject:
                HandleStartObject();
                break;
            case JsonTokenType.EndObject:
                HandleEndObject();
                break;
            case JsonTokenType.StartArray:
                break;
            case JsonTokenType.EndArray:
                break;
            case JsonTokenType.Comment:
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
        }

        return true;
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

    public DateTime GetDateTime()
    {
        throw new NotImplementedException(); 
    }
    
    public double GetDouble()
    {
        ValidateTaggedType(TokenType.Double);
        
         try
         { 
             return double.Parse(_taggedTokenValue, CultureInfo.InvariantCulture);
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
            return decimal.Parse(_taggedTokenValue, CultureInfo.InvariantCulture);
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
            return int.Parse(_taggedTokenValue);
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
            return long.Parse(_taggedTokenValue);
        }
        catch (Exception e)
        {
            throw new SerializationException($"Failed to get long from {_taggedTokenValue}", e);
        }
    }
    
    
    public Module GetModule()
    {
        throw new NotImplementedException();
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
                        
        switch (_json.GetString())
        {
            case "@date":
                throw new NotImplementedException();
            case "@doc":
                HandleTaggedStart(TokenType.StartDocument);
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
                throw new NotImplementedException();
            case "@object":
                throw new NotImplementedException();
            case "@ref":
                throw new NotImplementedException();
            case "@set":
                throw new NotImplementedException();
            case "@time":
                throw new NotImplementedException();
            default:
                _bufferedStartObject = true;
                _tokenStack.Push(TokenType.StartObject);
                CurrentTokenType = TokenType.StartObject;
                break;
        }
    }

    private void HandleEndObject()
    {
        switch (_tokenStack.Pop())
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
            case TokenType.StartObject:
            case TokenType.StartEscapedObject:
                CurrentTokenType = TokenType.EndObject;
                break;
            default:
                throw new SerializationException("This is a bug. Unhandled object wrapper token.");
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
    
    /// <summary>
    /// Method <c>HandleTaggedStart</c> is used to advance through JsonTokenType.StartObject when reading a tagged typed
    /// with an object representation. For, example:
    /// 
    /// * Given { "@doc": { "id": "123", "data": {}} }
    /// * Read JSON until JsonTokenType.PropertyName and you've determined it's a document
    /// * Call HandleTaggedStart(TokenType.Document)
    /// * The underlying JSON reader is advanced through the next JsonTokenType.StartObject
    /// * TokenType.StartDocument is pushed onto a stack to track that we're in a document
    /// * The Utf8FaunaReader is now ready to serve the contents of the document
    /// * 
    /// * Access the int via GetInt()
    /// 
    /// </summary>
    private void HandleTaggedStart(TokenType token)
    {
        AdvanceTrue();
        CurrentTokenType = token;
        _tokenStack.Push(token);
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