using System.Buffers;
using System.Text.Json;
using Fauna.Types;

namespace Fauna.Serialization;

public ref struct Utf8FaunaReader
{
    private Utf8JsonReader _json;
    private readonly Stack<TokenType> _tokenStack = new();
    private bool _bufferedStartObject = false;

    public TokenType TokenType { get; private set; }
    
    public Utf8FaunaReader(ReadOnlySequence<byte> bytes)
    {
        TokenType = TokenType.None;
        _json = new Utf8JsonReader(bytes);
    }

    public bool Read()
    {
        try
        {
            if (_bufferedStartObject)
            {
                _bufferedStartObject = false;
                TokenType = TokenType.FieldName;
                return true;
            }
            
            if (!_json.Read())
            {
                return false;
            }
            
            switch (_json.TokenType)
                {
                    case JsonTokenType.PropertyName:
                    {
                        TokenType = TokenType.FieldName;
                        break;
                    }
                    case JsonTokenType.Number:
                    {
                        throw new NotImplementedException();
                    }
                    case JsonTokenType.None:
                        break;
                    case JsonTokenType.StartObject:
                        // advance to determine if it's a tagged type
                        _json.Read();
                        
                        switch (_json.GetString())
                        {
                            case "@date":
                                throw new NotImplementedException();
                            case "@doc":
                                TokenType = TokenType.StartDocument;
                                _tokenStack.Push(TokenType.StartTaggedObject);
                                _tokenStack.Push(TokenType.StartDocument);
                                // advance through JsonTokenType.StartObject
                                return _json.Read();
                            case "@double":
                                throw new NotImplementedException();
                            case "@int":
                                throw new NotImplementedException();
                            case "@long":
                                throw new NotImplementedException();
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
                                TokenType = TokenType.StartObject;
                                break;
                        }
                        
                        break;
                    case JsonTokenType.EndObject:
                        switch (_tokenStack.Pop())
                        {
                            case TokenType.StartDocument:
                                TokenType = TokenType.EndDocument;
                                Read();
                                break;
                            case TokenType.StartSet:
                                TokenType = TokenType.EndSet;
                                Read();
                                break;
                            case TokenType.StartRef:
                                TokenType = TokenType.EndRef;
                                Read();
                                break;
                            case TokenType.StartObject:
                            case TokenType.StartEscapedObject:
                                TokenType = TokenType.EndObject;
                                break;
                            case TokenType.StartTaggedObject:
                                // we've advanced the Read() to pop this. Drop it on the floor.
                                break;
                            default:
                                throw new SerializationException("This is a bug. Unhandled object wrapper token.");
                        }

                        break;
                    case JsonTokenType.StartArray:
                        break;
                    case JsonTokenType.EndArray:
                        break;
                    case JsonTokenType.Comment:
                        break;
                    case JsonTokenType.String:
                        TokenType = TokenType.String;
                        break;
                    case JsonTokenType.True:
                        TokenType = TokenType.True;
                        break;
                    case JsonTokenType.False:
                        TokenType = TokenType.False;
                        break;
                    case JsonTokenType.Null:
                        TokenType = TokenType.Null;
                        break;
                }

            return true;
        }
        catch (JsonException e)
        {
            throw new SerializationException(e.Message);
        }
    }
    
    public void Skip()
    {
        throw new NotImplementedException();
    }

    public string? GetString()
    {
        if (_bufferedStartObject)
        {
            // if we buffered StartObject, behave like we're at StartObject.
            // TODO: Write a nice message.
            throw new InvalidOperationException();
        }
        
        // TODO: Wrap these errors in our own.
        return _json.GetString();
    }
    
    public bool GetBoolean()
    {
        throw new NotImplementedException(); 
    }

    public DateTime GetDateTime()
    {
        throw new NotImplementedException(); 
    }
    
    public double GetDouble()
    {
        throw new NotImplementedException();
    }
    
    public int GetInt()
    {
        throw new NotImplementedException();
    }
    
    public long GetLong()
    {
        throw new NotImplementedException();
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
}