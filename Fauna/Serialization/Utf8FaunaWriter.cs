using System.Buffers;
using System.Globalization;
using System.Text.Json;
using Fauna.Serialization.Attributes;
using Fauna.Types;

namespace Fauna.Serialization;

public sealed class Utf8FaunaWriter: IAsyncDisposable, IDisposable
{
    private readonly Utf8JsonWriter _writer;
    public Utf8FaunaWriter(IBufferWriter<byte> bufferWriter)
    {
        _writer = new Utf8JsonWriter(bufferWriter);
    }
    
    public Utf8FaunaWriter(Stream stream)
    {
        _writer = new Utf8JsonWriter(stream);
    }

    public void Flush()
    {
        _writer.Flush();
    }
    
    public async ValueTask FlushAsync()
    {
        await _writer.FlushAsync();
    }
    
    public void Dispose()
    {
        _writer.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _writer.DisposeAsync();
    }

    public void WriteStartObject()
    {
        _writer.WriteStartObject();
    }
    
    public void WriteEndObject()
    {
        _writer.WriteEndObject();
    }
    
    public void WriteStartEscapedObject()
    {
        _writer.WriteStartObject();
        WriteFieldName("@object");
        _writer.WriteStartObject();
    }
    
    public void WriteEndEscapedObject()
    {
        _writer.WriteEndObject();
        _writer.WriteEndObject();
    }
    
    public void WriteStartArray()
    {
        _writer.WriteStartArray();
    }
    
    public void WriteEndArray()
    {
        _writer.WriteEndArray();
    }

    public void WriteStartRef()
    {
        _writer.WriteStartObject();
        WriteFieldName("@ref");
        _writer.WriteStartObject();
    }

    public void WriteEndRef()
    {
        _writer.WriteEndObject();
        _writer.WriteEndObject();
    }

    public void WriteDouble(string fieldName, decimal value)
    {
        WriteFieldName(fieldName);
        WriteDoubleValue(value);
    }
    
    public void WriteDouble(string fieldName, double value)
    {
        WriteFieldName(fieldName);
        WriteDoubleValue(value);
    }

    public void WriteInt(string fieldName, int value)
    {
        WriteFieldName(fieldName);
        WriteIntValue(value);
    }
    
    public void WriteLong(string fieldName, long value)
    {
        WriteFieldName(fieldName);
        WriteLongValue(value);
    }
    
    public void WriteString(string fieldName, string value)
    {
        WriteFieldName(fieldName);
        WriteStringValue(value);
    }
    
    public void WriteDate(string fieldName, DateTime value)
    {
        WriteFieldName(fieldName);
        WriteDateValue(value);
    }
    
    public void WriteTime(string fieldName, DateTime value)
    {
        WriteFieldName(fieldName);
        WriteTimeValue(value);
    }
    
    public void WriteBoolean(string fieldName, bool value)
    {
        WriteFieldName(fieldName);
        WriteBooleanValue(value);
        
    }
    
    public void WriteNull(string fieldName)
    {
        WriteFieldName(fieldName);
        WriteNullValue();
    }
    
    public void WriteModule(string fieldName, Module value)
    {
        WriteFieldName(fieldName);
        WriteModuleValue(value);
    }

    public void WriteFieldName(string value)
    {
        _writer.WritePropertyName(value);
    }

    public void WriteTaggedValue(string tag, string value)
    {
        WriteStartObject();
        WriteString(tag, value);
        WriteEndObject();
    }
    
    public void WriteDoubleValue(decimal value)
    {
        WriteTaggedValue("@double", value.ToString(CultureInfo.InvariantCulture));
    }
    
    public void WriteDoubleValue(double value)
    {
        WriteTaggedValue("@double", value.ToString(CultureInfo.InvariantCulture));
    }
    
    public void WriteIntValue(int value)
    {
        WriteTaggedValue("@int", value.ToString());
    }
    
    public void WriteLongValue(long value)
    {
        WriteTaggedValue("@long", value.ToString());
    }
    
    public void WriteStringValue(string value)
    {
        _writer.WriteStringValue(value);
    }
    
    
    public void WriteDateValue(DateTime value)
    {
        var str = value.ToString("yyyy-MM-dd");
        WriteTaggedValue("@date", str);
    }
    
    public void WriteTimeValue(DateTime value)
    {
        var str = value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
        WriteTaggedValue("@time", str);
    }
    
    public void WriteBooleanValue(bool value)
    {
        _writer.WriteBooleanValue(value);
    }
    
    public void WriteNullValue()
    {
        _writer.WriteNullValue();
    }
    
    public void WriteModuleValue(Module value)
    {
        WriteTaggedValue("@mod", value.Name);
    }
}