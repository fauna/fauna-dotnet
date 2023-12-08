using System.Buffers;
using System.Text;
using Fauna.Serialization;
using NUnit.Framework;

namespace Fauna.Test;

[TestFixture]
public class ClientTests
{
    private void Write(string json)
    {
        var reader = new Utf8FaunaReader(new ReadOnlySequence<byte>(Encoding.UTF8.GetBytes(json)));

        while (reader.Read())
        {
            Console.WriteLine($"Type: {reader.CurrentTokenType}");
        }
    }

    [Test]
    public async Task CreateClient()
    {
        var t = new { data = new { data = Array.Empty<object>() } };
        var c = new Client(
            new ClientConfig("secret")
            {
                Endpoint = Constants.Endpoints.Local,
                DefaultQueryOptions = new QueryOptions
                {
                    QueryTags = new Dictionary<string, string> { { "lorem", "ipsum" } }
                }
            });
        var r = await c.QueryAsync<string>(
            "let x = 123; x",
            new QueryOptions { QueryTags = new Dictionary<string, string> { { "foo", "bar" }, {"baz", "luhrmann"} }});
        Write(r.Data);
        Console.WriteLine(string.Join(',',r.QueryTags!.Select(kv => $"{kv.Key}={kv.Value}")));
    }
}
