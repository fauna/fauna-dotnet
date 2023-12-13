namespace Fauna;

public interface IConnection
{
    Task<QueryResponse> DoPostAsync<T>(
        string path,
        Stream body,
        Dictionary<string, string> headers);
}
