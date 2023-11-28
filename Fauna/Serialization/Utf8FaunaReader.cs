using System.Buffers;
using System.Text.Json;
using Fauna.Types;

namespace Fauna.Serialization;

public ref struct Utf8FaunaReader
{

    private Utf8JsonReader _json;
    
    public FaunaTokenType FaunaTokenType { get; }
    
    public Utf8FaunaReader(ReadOnlySequence<byte> bytes)
    {
        FaunaTokenType = FaunaTokenType.None;
        _json = new Utf8JsonReader(bytes);
    }

    public bool Read()
    {
        throw new NotImplementedException();
    }
    
    public bool Skip()
    {
        throw new NotImplementedException();
    }

    public string GetString()
    {
        throw new NotImplementedException(); 
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