using System.Globalization;
using Fauna.Configuration;
using Fauna.Constants;
using Fauna.Encoding;

namespace Fauna;

public class RequestBuilder
{
    private readonly Config _faunaConfig;
    private readonly Auth _auth;
    private readonly Uri _uri;

    private RequestBuilder(Builder builder)
    {
        _faunaConfig = builder.FaunaConfig;
        _uri = _faunaConfig.Endpoint;
        _auth = new Auth(_faunaConfig.Secret);
    }

    public static Builder CreateBuilder()
    {
        return new Builder();
    }

    public HttpRequestMessage BuildRequest(string fql)
    {
        var headers = BuildHeaders();
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _uri)
        {
            Content = new StringContent(fql)
        };

        foreach (var header in headers)
        {
            httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return httpRequest;
    }

    private Dictionary<string, string> BuildHeaders()
    {
        var headers = new Dictionary<string, string>
        {
            { Headers.Authorization, _auth.Bearer() },
            { Headers.Format, "tagged" },
            { Headers.AcceptEncoding, "gzip" },
            { Headers.ContentType, "application/json;charset=utf-8" },
            { Headers.Driver, "C#" },
            {
                Headers.QueryTimeoutMs,
                _faunaConfig.QueryTimeout.TotalMilliseconds.ToString(CultureInfo.InvariantCulture)
            }
        };

        if (_faunaConfig.Linearized != null)
        {
            headers.Add(Headers.Linearized, _faunaConfig.Linearized.ToString());
        }

        if (_faunaConfig.TypeCheck != null)
        {
            headers.Add(Headers.TypeCheck, _faunaConfig.TypeCheck.ToString());
        }

        if (_faunaConfig.QueryTags != null)
        {
            headers.Add(Headers.QueryTags, QueryTags.Encode(_faunaConfig.QueryTags));
        }

        if (!string.IsNullOrEmpty(_faunaConfig.TraceParent))
        {
            headers.Add(Headers.TraceParent, _faunaConfig.TraceParent);
        }

        return headers;
    }

    public class Builder
    {
        public Config? FaunaConfig { get; private set; }

        public Builder SetFaunaConfig(Config faunaConfig)
        {
            FaunaConfig = faunaConfig;
            return this;
        }

        public RequestBuilder Build()
        {
            return new RequestBuilder(this);
        }
    }
}
