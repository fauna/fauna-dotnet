using NUnit.Framework;
using System.Text.Json;

namespace Fauna.Test;

[TestFixture]
public class ClientTests
{
    private class Result
    {
        public Data data {get;set;}
    }

    private class Data
    {
        public List<object> data {get;set;}
    }

    [Test]
    public async Task CreateClient()
    {
        var t = new { data = new { data = Array.Empty<object>() } };
        var c = new Client(new ClientConfig { Secret = "secret", Endpoint = Constants.Endpoints.Local });
        var r = await c.QueryAsync("Collection.all()");
        var j = JsonSerializer.Deserialize<Result>(r);

        Console.WriteLine(j.data.data.First());
    }
}
