using static Fauna.Query;

namespace Fauna.Test.Helpers;

public static class TestClientHelper
{
    public static Client NewTestClient()
    {
        var cfg = new Configuration("secret")
        {
            Endpoint = new Uri("http://localhost:8443"),
        };

        return new Client(cfg);
    }
}
