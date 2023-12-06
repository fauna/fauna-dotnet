namespace Fauna;

internal interface IConnection
{
    long LastSeenTxn { get; set; }

    Task<HttpResponseMessage> DoRequestAsync(
        string fql,
        int queryTimeoutSeconds,
        Dictionary<string, string>? queryTags,
        string? traceParent);
}
