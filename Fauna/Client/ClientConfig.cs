namespace Fauna;

/// <summary>
/// FaunaConfig is a configuration class used to set up and configure a connection to Fauna.
/// It encapsulates various settings such as the endpoint URL, secret key, query timeout, and others.
/// </summary>
public class ClientConfig
{
    public Uri Endpoint { get; }
    public string Secret { get; }
    public bool? Linearized { get; }
    public bool? TypeCheck { get; }
    public TimeSpan QueryTimeout { get; }
    public string? TraceParent { get; }
    public Dictionary<string, string>? QueryTags { get; }

    private static readonly TimeSpan DefaultQueryTimeout = TimeSpan.FromSeconds(5);

    private ClientConfig(Builder builder)
    {
        Endpoint = builder.Endpoint;
        Secret = builder.Secret;
        QueryTimeout = builder.QueryTimeout;
        Linearized = builder.Linearized;
        TypeCheck = builder.TypeCheck;
        QueryTags = builder.QueryTags;
        TraceParent = builder.TraceParent;
    }

    public static Builder CreateBuilder()
    {
        return new Builder();
    }

    public class Builder
    {
        public Uri Endpoint { get; private set; } = new Uri(Constants.Endpoints.Default);
        public string Secret { get; private set; }
        public TimeSpan QueryTimeout { get; private set; } = DefaultQueryTimeout;
        public bool? Linearized { get; private set; }
        public bool? TypeCheck { get; private set; }
        public Dictionary<string, string>? QueryTags { get; private set; }
        public string? TraceParent { get; private set; }

        public Builder SetEndpoint(string endpoint)
        {
            Endpoint = new Uri(endpoint);
            return this;
        }

        public Builder SetSecret(string secret)
        {
            Secret = secret;
            return this;
        }

        public Builder SetQueryTimeout(TimeSpan queryTimeout)
        {
            QueryTimeout = queryTimeout;
            return this;
        }

        public Builder SetLinearized(bool? linearized)
        {
            Linearized = linearized;
            return this;
        }

        public Builder SetTypeCheck(bool? typeCheck)
        {
            TypeCheck = typeCheck;
            return this;
        }

        public Builder SetQueryTags(Dictionary<string, string> queryTags)
        {
            QueryTags = queryTags;
            return this;
        }

        public Builder SetTraceParent(string traceParent)
        {
            TraceParent = traceParent;
            return this;
        }

        public ClientConfig Build()
        {
            return new ClientConfig(this);
        }
    }
}
