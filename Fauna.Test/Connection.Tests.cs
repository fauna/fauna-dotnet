using NUnit.Framework;
using Telerik.JustMock;

namespace Fauna.Tests;

[TestFixture]
public class ConnectionTests
{
    [Test]
    public void Ctor_WithEndpoint()
    {
        var endpoint = new Uri("http://localhost:8443");
        var connectionTimeout = TimeSpan.FromSeconds(30);
        var maxRetries = 3;
        var maxBackoff = TimeSpan.FromSeconds(10);
        var connection = new Connection(endpoint, connectionTimeout, maxRetries, maxBackoff);
        Assert.NotNull(connection);
    }

    [Test]
    public void Ctor_WithHttpClient()
    {
        var maxRetries = 3;
        var maxBackoff = TimeSpan.FromSeconds(10);
        var mockHttpClient = Mock.Create<HttpClient>();
        var connection = new Connection(mockHttpClient, maxRetries, maxBackoff);
        Assert.NotNull(connection);
    }

    [Test]
    public void Dispose_ShouldNotDisposeExternalHttpClient()
    {
        var maxRetries = 3;
        var maxBackoff = TimeSpan.FromSeconds(10);
        var mockHttpClient = Mock.Create<HttpClient>();
        var connection = new Connection(mockHttpClient, maxRetries, maxBackoff);
        connection.Dispose();
        Mock.Assert(() => mockHttpClient.Dispose(), Occurs.Never());
    }
}
