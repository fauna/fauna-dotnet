using System.Globalization;
using System.Net.Http.Json;
using Fauna.Constants;

namespace Fauna;

internal sealed class RequestBuilder
{
    private const string QueryPath = "/query/1";
    private readonly ClientConfig _clientConfig;

    internal RequestBuilder(ClientConfig config)
    {
        _clientConfig = config;
    }

    public HttpRequestMessage BuildRequest(string fql)
    {
        var queryObj = new
        {
            query = new
            {
                fql = new[] { fql }
            },
            arguments = new { }
        };
        var headers = GetRequestHeaders();
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, QueryPath)
        {
            Content = JsonContent.Create(queryObj)
        };

        foreach (var header in headers)
        {
            httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return httpRequest;
    }

    private Dictionary<string, string> GetRequestHeaders()
    {
        var headers = new Dictionary<string, string>
        {
            { Headers.Authorization, $"Bearer {_clientConfig.Secret}" },
            { Headers.Format, "simple" },
            { Headers.AcceptEncoding, "gzip" },
            { Headers.ContentType, "application/json;charset=utf-8" },
            { Headers.Driver, "C#" },
            {
                Headers.QueryTimeoutMs,
                _clientConfig.QueryTimeout.TotalMilliseconds.ToString(CultureInfo.InvariantCulture)
            }
        };

        if (_clientConfig.Linearized != null)
        {
            headers.Add(Headers.Linearized, _clientConfig.Linearized.ToString());
        }

        if (_clientConfig.TypeCheck != null)
        {
            headers.Add(Headers.TypeCheck, _clientConfig.TypeCheck.ToString());
        }

        if (_clientConfig.QueryTags != null)
        {
            headers.Add(Headers.QueryTags, EncodeQueryTags(_clientConfig.QueryTags));
        }

        if (!string.IsNullOrEmpty(_clientConfig.TraceParent))
        {
            headers.Add(Headers.TraceParent, _clientConfig.TraceParent);
        }

        return headers;
    }

    private string EncodeQueryTags(Dictionary<string, string> tags)
    {
        return string.Join(",", tags.Select(entry => entry.Key + "=" + entry.Value));
    }
}
