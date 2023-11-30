namespace Fauna.Constants;

public readonly struct Endpoints
{
    public static readonly Uri Default = new("https://db.fauna.com");
    public static readonly Uri Local = new("http://localhost:8443");
}
