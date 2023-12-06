using System.Net.Http.Headers;
using System.Net.Http.Json;
using Fauna.Constants;

namespace Fauna;

internal class Connection : IConnection
{
    private const string QueryUriPath = "/query/1";
    private readonly ClientConfig _config;
    private readonly HttpClient _httpClient;

    public long LastSeenTxn { get; set; }

    internal Connection(ClientConfig config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }

    public async Task<HttpResponseMessage> DoRequestAsync(
        string fql,
        int queryTimeoutSeconds,
        Dictionary<string, string>? queryTags,
        string? traceParent)
    {
        var request = GetHttpRequestMessage(fql);
        var requestHeaders = GetRequestHeaders();

        requestHeaders.Add(Headers.QueryTimeoutMs, (queryTimeoutSeconds * 1000).ToString());

        if (queryTags != null)
        {
            requestHeaders.Add(Headers.QueryTags, EncodeQueryTags(queryTags));
        }

        if (!string.IsNullOrEmpty(traceParent))
        {
            requestHeaders.Add(Headers.TraceParent, traceParent);
        }

        if (LastSeenTxn > long.MinValue)
        {
            requestHeaders.Add(Headers.LastTxnTs, LastSeenTxn.ToString());
        }

        foreach (var header in requestHeaders)
        {
            request.Headers.Add(header.Key, header.Value);
        }

        return await _httpClient.SendAsync(request);
    }

    private HttpRequestMessage GetHttpRequestMessage(string fql)
    {
        var queryObj = new
        {
            query = new
            {
                fql = new[] { fql }
            },
            arguments = new { }
        };

        var httpRequest = new HttpRequestMessage()
        {
            Content = JsonContent.Create(queryObj),
            Method = HttpMethod.Post,
            RequestUri = new Uri(_config.Endpoint, QueryUriPath)
        };

        // Configure standard headers
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.Secret);
        httpRequest.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return httpRequest;
    }

    private Dictionary<string, string> GetRequestHeaders()
    {
        var headers = new Dictionary<string, string>
        {
            { Headers.Format, "tagged" },
            { Headers.Driver, "C#" }
        };

        if (_config.Linearized != null)
        {
            headers.Add(Headers.Linearized, _config.Linearized.ToString()!);
        }

        if (_config.TypeCheck != null)
        {
            headers.Add(Headers.TypeCheck, _config.TypeCheck.ToString()!);
        }

        return headers;
    }

    private string EncodeQueryTags(Dictionary<string, string> tags)
    {
        return string.Join(",", tags.Select(entry => entry.Key + "=" + entry.Value));
    }
}
