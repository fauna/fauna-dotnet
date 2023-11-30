using System.Globalization;
using System.Net.Http.Json;
using Fauna.Constants;

namespace Fauna;

internal class RequestBuilder
{
    private const string QueryPath = "/query/1";
    private readonly ClientConfig _config;

    internal RequestBuilder(ClientConfig config)
    {
        _config = config;
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
            { Headers.Authorization, $"Bearer {_config.Secret}" },
            { Headers.Format, "simple" },
            { Headers.AcceptEncoding, "gzip" },
            { Headers.ContentType, "application/json;charset=utf-8" },
            { Headers.Driver, "C#" },
            {
                Headers.QueryTimeoutMs,
                _config.QueryTimeout.TotalMilliseconds.ToString(CultureInfo.InvariantCulture)
            }
        };

        if (_config.Linearized != null)
        {
            headers.Add(Headers.Linearized, _config.Linearized.ToString());
        }

        if (_config.TypeCheck != null)
        {
            headers.Add(Headers.TypeCheck, _config.TypeCheck.ToString());
        }

        if (_config.QueryTags != null)
        {
            headers.Add(Headers.QueryTags, EncodeQueryTags(_config.QueryTags));
        }

        if (!string.IsNullOrEmpty(_config.TraceParent))
        {
            headers.Add(Headers.TraceParent, _config.TraceParent);
        }

        return headers;
    }

    private string EncodeQueryTags(Dictionary<string, string> tags)
    {
        return string.Join(",", tags.Select(entry => entry.Key + "=" + entry.Value));
    }
}
