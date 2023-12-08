namespace Fauna;

internal interface IConnection
{
    long LastSeenTxn { get; set; }

    Task<HttpResponseMessage> DoRequestAsync(
        string fql,
        QueryOptions? queryOptions);
}
