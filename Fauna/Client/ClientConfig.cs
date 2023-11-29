namespace Fauna;

/// <summary>
/// FaunaConfig is a configuration class used to set up and configure a connection to Fauna.
/// It encapsulates various settings such as the endpoint URL, secret key, query timeout, and others.
/// </summary>
public class ClientConfig
{
    public string Secret { get; init; } = "secret";
    public Uri Endpoint { get; init; } = Constants.Endpoints.Default;
    public bool? Linearized { get; init; } = null;
    public bool? TypeCheck { get; init; } = null;
    public TimeSpan QueryTimeout { get; init; } = DefaultQueryTimeout;
    public TimeSpan ConnectionTimeout { get; init; } = DefaultConnectionTimeout;
    public string? TraceParent { get; init; }
    public Dictionary<string, string>? QueryTags { get; init; }

    private static readonly TimeSpan DefaultQueryTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan DefaultConnectionTimeout = TimeSpan.FromSeconds(30);
}
