namespace Fauna;

public interface IConnection
{
    Task<HttpResponseMessage> DoPostAsync(
        string path,
        Stream body,
        Dictionary<string, string> headers);
}
