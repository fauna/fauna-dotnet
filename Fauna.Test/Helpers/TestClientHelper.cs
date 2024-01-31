using static Fauna.Query;

namespace Fauna.Test.Helpers;

public static class TestClientHelper
{
    public static Connection NewTestConnection() =>
        new Connection(
            new Uri("http://localhost:8443"),
            TimeSpan.FromSeconds(5),
            3,
            TimeSpan.FromSeconds(10)
        );

    public static Client NewTestClient() => NewTestClient(NewTestConnection());

    public static Client NewTestClient(Connection conn) =>
        new Client(new ClientConfig("secret"), conn);
}
