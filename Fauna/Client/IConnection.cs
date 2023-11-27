namespace Fauna;

public interface IConnection
{
    Task<HttpResponseMessage> PerformRequestAsync(string fql);
}
