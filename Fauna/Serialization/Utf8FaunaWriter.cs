using System.Buffers;
using System.Globalization;
using System.Text.Json;
using Fauna.Types;
using Stream = System.IO.Stream;

namespace Fauna.Serialization;

/// <summary>
/// Provides functionality for writing data in a streaming manner to a buffer or a stream.
/// </summary>
public sealed class Utf8FaunaWriter : IAsyncDisposable, IDisposable
{
    private readonly Utf8JsonWriter _writer;

    /// <summary>
    /// Initializes a new instance of the Utf8FaunaWriter class with a specified buffer writer.
    /// </summary>
    /// <param name="bufferWriter">The buffer writer to write to.</param>
    public Utf8FaunaWriter(IBufferWriter<byte> bufferWriter)
    {
        _writer = new Utf8JsonWriter(bufferWriter);
    }

    /// <summary>
    /// Initializes a new instance of the Utf8FaunaWriter class with a specified stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    public Utf8FaunaWriter(Stream stream)
    {
        _writer = new Utf8JsonWriter(stream);
    }

    /// <summary>
    /// Flushes the written data to the underlying buffer or stream.
    /// </summary>
    public void Flush()
    {
        _writer.Flush();
    }

    /// <summary>
    /// Asynchronously flushes the written data to the underlying buffer or stream.
    /// </summary>
    public async ValueTask FlushAsync()
    {
        await _writer.FlushAsync();
    }

    /// <summary>
    /// Disposes the underlying writer.
    /// </summary>
    public void Dispose()
    {
        _writer.Dispose();
    }

    /// <summary>
    /// Asynchronously disposes the underlying writer.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _writer.DisposeAsync();
    }

    /// <summary>
    /// Writes the beginning of an object.
    /// </summary>
    public void WriteStartObject()
    {
        _writer.WriteStartObject();
    }

    /// <summary>
    /// Writes the end of an object.
    /// </summary>
    public void WriteEndObject()
    {
        _writer.WriteEndObject();
    }

    /// <summary>
    /// Writes the beginning of a specially tagged object.
    /// </summary>
    public void WriteStartEscapedObject()
    {
        _writer.WriteStartObject();
        WriteFieldName("@object");
        _writer.WriteStartObject();
    }

    /// <summary>
    /// Writes the end of a specially tagged object.
    /// </summary>
    public void WriteEndEscapedObject()
    {
        _writer.WriteEndObject();
        _writer.WriteEndObject();
    }

    /// <summary>
    /// Writes the beginning of an array.
    /// </summary>
    public void WriteStartArray()
    {
        _writer.WriteStartArray();
    }

    /// <summary>
    /// Writes the end of an array.
    /// </summary>
    public void WriteEndArray()
    {
        _writer.WriteEndArray();
    }

    /// <summary>
    /// Writes the beginning of a reference object.
    /// </summary>
    public void WriteStartRef()
    {
        _writer.WriteStartObject();
        WriteFieldName("@ref");
        _writer.WriteStartObject();
    }

    /// <summary>
    /// Writes the end of a reference object.
    /// </summary>
    public void WriteEndRef()
    {
        _writer.WriteEndObject();
        _writer.WriteEndObject();
    }

    /// <summary>
    /// Writes a double value with a specific field name.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">The decimal value to write.</param>
    public void WriteDouble(string fieldName, decimal value)
    {
        WriteFieldName(fieldName);
        WriteDoubleValue(value);
    }

    /// <summary>
    /// Writes a double value with a specific field name.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">The double value to write.</param>
    public void WriteDouble(string fieldName, double value)
    {
        WriteFieldName(fieldName);
        WriteDoubleValue(value);
    }

    /// <summary>
    /// Writes an integer value with a specific field name.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">The integer value to write.</param>
    public void WriteInt(string fieldName, int value)
    {
        WriteFieldName(fieldName);
        WriteIntValue(value);
    }

    /// <summary>
    /// Writes a long integer value with a specific field name.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">The long integer value to write.</param>
    public void WriteLong(string fieldName, long value)
    {
        WriteFieldName(fieldName);
        WriteLongValue(value);
    }

    /// <summary>
    /// Writes a string value with a specific field name.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">The string value to write.</param>
    public void WriteString(string fieldName, string value)
    {
        WriteFieldName(fieldName);
        WriteStringValue(value);
    }

    /// <summary>
    /// Writes a date value with a specific field name.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">The DateTime value to write.</param>
    public void WriteDate(string fieldName, DateTime value)
    {
        WriteFieldName(fieldName);
        WriteDateValue(value);
    }

    /// <summary>
    /// Writes a time value with a specific field name.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">The DateTime value to write.</param>
    public void WriteTime(string fieldName, DateTime value)
    {
        WriteFieldName(fieldName);
        WriteTimeValue(value);
    }

    /// <summary>
    /// Writes a boolean value with a specific field name.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">The boolean value to write.</param>
    public void WriteBoolean(string fieldName, bool value)
    {
        WriteFieldName(fieldName);
        WriteBooleanValue(value);

    }

    /// <summary>
    /// Writes a null value with a specific field name.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    public void WriteNull(string fieldName)
    {
        WriteFieldName(fieldName);
        WriteNullValue();
    }

    /// <summary>
    /// Writes a module value with a specific field name.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="value">The module value to write.</param>
    public void WriteModule(string fieldName, Module value)
    {
        WriteFieldName(fieldName);
        WriteModuleValue(value);
    }

    /// <summary>
    /// Writes a field name for the next value.
    /// </summary>
    /// <param name="value">The name of the field.</param>
    public void WriteFieldName(string value)
    {
        _writer.WritePropertyName(value);
    }

    /// <summary>
    /// Writes a tagged value in an object.
    /// </summary>
    /// <param name="tag">The tag to use for the value.</param>
    /// <param name="value">The value associated with the tag.</param>
    public void WriteTaggedValue(string tag, string value)
    {
        WriteStartObject();
        WriteString(tag, value);
        WriteEndObject();
    }

    /// <summary>
    /// Writes a double value as a tagged element.
    /// </summary>
    /// <param name="value">The double value to write.</param>
    public void WriteDoubleValue(decimal value)
    {
        WriteTaggedValue("@double", value.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Writes a double value as a tagged element.
    /// </summary>
    /// <param name="value">The double value to write.</param>
    public void WriteDoubleValue(double value)
    {
        WriteTaggedValue("@double", value.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Writes an integer value as a tagged element.
    /// </summary>
    /// <param name="value">The integer value to write.</param>
    public void WriteIntValue(int value)
    {
        WriteTaggedValue("@int", value.ToString());
    }

    /// <summary>
    /// Writes a long integer value as a tagged element.
    /// </summary>
    /// <param name="value">The long integer value to write.</param>
    public void WriteLongValue(long value)
    {
        WriteTaggedValue("@long", value.ToString());
    }

    /// <summary>
    /// Writes a string value as a tagged element.
    /// </summary>
    /// <param name="value">The string value to write.</param>
    public void WriteStringValue(string value)
    {
        _writer.WriteStringValue(value);
    }

    /// <summary>
    /// Writes a date value as a tagged element.
    /// </summary>
    /// <param name="value">The date value to write.</param>
    public void WriteDateValue(DateTime value)
    {
        var str = value.ToString("yyyy-MM-dd");
        WriteTaggedValue("@date", str);
    }

    /// <summary>
    /// Writes a date value as a tagged element.
    /// </summary>
    /// <param name="value">The date value to write.</param>
    public void WriteDateValue(DateOnly value)
    {
        var str = value.ToString("yyyy-MM-dd");
        WriteTaggedValue("@date", str);
    }

    /// <summary>
    /// Writes a date value as a tagged element.
    /// </summary>
    /// <param name="value">The date value to write.</param>
    public void WriteDateValue(DateTimeOffset value)
    {
        var str = value.ToString("yyyy-MM-dd");
        WriteTaggedValue("@date", str);
    }

    /// <summary>
    /// Writes a date value as a tagged element.
    /// </summary>
    /// <param name="value">The date value to write.</param>
    public void WriteTimeValue(DateTime value)
    {
        var str = value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
        WriteTaggedValue("@time", str);
    }

    /// <summary>
    /// Writes a date value as a tagged element.
    /// </summary>
    /// <param name="value">The date value to write.</param>
    public void WriteTimeValue(DateTimeOffset value)
    {
        var str = value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
        WriteTaggedValue("@time", str);
    }

    /// <summary>
    /// Writes a boolean value to the stream.
    /// </summary>
    /// <param name="value">The boolean value to write.</param>
    public void WriteBooleanValue(bool value)
    {
        _writer.WriteBooleanValue(value);
    }

    /// <summary>
    /// Writes a null value to the stream.
    /// </summary>
    public void WriteNullValue()
    {
        _writer.WriteNullValue();
    }

    /// <summary>
    /// Writes a module value as a tagged element.
    /// </summary>
    /// <param name="value">The module value to write.</param>
    public void WriteModuleValue(Module value)
    {
        WriteTaggedValue("@mod", value.Name);
    }
}