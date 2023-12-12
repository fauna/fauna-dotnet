namespace Fauna;

public interface IConnection
{
    Task<QueryResponse> DoPostAsync<T>(
        string path,
        string body,
        Dictionary<string, string> headers)
        where T : class;
}
