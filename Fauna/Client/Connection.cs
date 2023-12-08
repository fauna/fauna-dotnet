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
        QueryOptions? queryOptions)
    {
        var finalOptions = QueryOptions.GetFinalQueryOptions(_config.DefaultQueryOptions, queryOptions);
        var request = GetHttpRequestMessage(fql);
        var requestHeaders = GetRequestHeaders(finalOptions);

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

    private Dictionary<string, string> GetRequestHeaders(QueryOptions? queryOptions)
    {
        var headers = new Dictionary<string, string>
        {
            { Headers.Format, "tagged" },
            { Headers.Driver, "C#" }
        };

        if (LastSeenTxn > long.MinValue)
        {
            headers.Add(Headers.LastTxnTs, LastSeenTxn.ToString());
        }

        if (queryOptions != null)
        {
            if (queryOptions.QueryTimeout.HasValue)
            {
                headers.Add(
                    Headers.QueryTimeoutMs,
                    queryOptions.QueryTimeout.Value.TotalMilliseconds.ToString());
            }

            if (queryOptions.QueryTags != null)
            {
                headers.Add(Headers.QueryTags, EncodeQueryTags(queryOptions.QueryTags));
            }

            if (!string.IsNullOrEmpty(queryOptions.TraceParent))
            {
                headers.Add(Headers.TraceParent, queryOptions.TraceParent);
            }

            if (queryOptions.Linearized != null)
            {
                headers.Add(Headers.Linearized, queryOptions.Linearized.ToString()!);
            }

            if (queryOptions.TypeCheck != null)
            {
                headers.Add(Headers.TypeCheck, queryOptions.TypeCheck.ToString()!);
            }
        }

        return headers;
    }

    private string EncodeQueryTags(Dictionary<string, string> tags)
    {
        return string.Join(",", tags.Select(entry => entry.Key + "=" + entry.Value));
    }
}
