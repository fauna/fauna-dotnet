namespace Fauna;

public interface IConnection
{
    Task<HttpResponseMessage> DoPostAsync(
        string path,
        string body,
        Dictionary<string, string> headers);
}
