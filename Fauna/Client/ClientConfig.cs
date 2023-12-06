namespace Fauna;

/// <summary>
/// ClientConfig is a configuration class used to set up and configure a connection to Fauna.
/// It encapsulates various settings such as the endpoint URL, secret key, query timeout, and others.
/// </summary>
public class ClientConfig
{
    public string Secret { get; init; }
    public Uri Endpoint { get; init; } = Constants.Endpoints.Default;
    public bool? Linearized { get; init; } = null;
    public bool? TypeCheck { get; init; } = null;
    public TimeSpan ConnectionTimeout { get; init; } = DefaultConnectionTimeout;

    private static readonly TimeSpan DefaultConnectionTimeout = TimeSpan.FromSeconds(60);

    public ClientConfig(string secret)
    {
        Secret = secret;
    }
}
