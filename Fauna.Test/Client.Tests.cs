using System.Buffers;
using System.Net;
using System.Text;
using Fauna.Constants;
using Fauna.Serialization;
using NUnit.Framework;
using Telerik.JustMock;

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
            new QueryOptions { QueryTags = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "luhrmann" } } });
        Write(r.Data);
        Console.WriteLine(string.Join(',', r.QueryTags!.Select(kv => $"{kv.Key}={kv.Value}")));
    }

    [Test]
    public async Task CreateClientError()
    {
        var t = new { data = new { data = Array.Empty<object>() } };
        var c = new Client(
            new ClientConfig("secret")
            {
                Endpoint = Endpoints.Local,
                DefaultQueryOptions = new QueryOptions
                {
                    QueryTags = new Dictionary<string, string> { { "lorem", "ipsum" } }
                }
            });

        try
        {
            var r = await c.QueryAsync<string>(
                "let x = 123; abort(x)",
                new QueryOptions { QueryTags = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "luhrmann" } } });
        }
        catch (FaunaException ex)
        {
            Assert.AreEqual("abort", ex.QueryFailure.ErrorInfo.Code);
            var abortNum = GetIntFromReader(ex.QueryFailure.ErrorInfo.Abort.ToString()!);
            Assert.AreEqual(123, abortNum);
            Console.WriteLine(ex.QueryFailure.Summary);
        }
    }

    [Test]
    public async Task MockedTestError()
    {
        var responseBody = @"{
            ""error"": {
                ""code"": ""testAbort"",
                ""message"": ""Query aborted."",
                ""abort"": ""123""
            },
            ""summary"": ""error: Query aborted.\nat *query*:1:19\n  |\n1 | let x = 123; abort(x)\n  |                   ^^^\n  |"",
            ""txn_ts"": 1702346199930000,
            ""stats"": {
                ""compute_ops"": 1,
                ""read_ops"": 0,
                ""write_ops"": 0,
                ""query_time_ms"": 105,
                ""contention_retries"": 0,
                ""storage_bytes_read"": 261,
                ""storage_bytes_write"": 0,
                ""rate_limits_hit"": []
            },
            ""schema_version"": 0
        }";
        var testMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(responseBody)
        };
        var conn = Mock.Create<IConnection>();
        Mock.Arrange(() =>
            conn.DoPostAsync(
                Arg.IsAny<string>(),
                Arg.IsAny<string>(),
                Arg.IsAny<Dictionary<string, string>>()
            )
        ).Returns(Task.FromResult(testMessage));

        var c = new Client(new ClientConfig("secret"), conn);

        try
        {
            var r = await c.QueryAsync<string>("let x = 123; abort(x)");
        }
        catch (FaunaException ex)
        {
            Assert.AreEqual("testAbort", ex.QueryFailure.ErrorInfo.Code);
        }
    }

    [Test]
    public async Task MockedTest()
    {
        var responseBody = @"{
            ""data"": ""123"",
            ""static_type"": ""123"",
            ""summary"": ""All good"",
            ""txn_ts"": 1702346199930000,
            ""stats"": {
                ""compute_ops"": 1,
                ""read_ops"": 0,
                ""write_ops"": 0,
                ""query_time_ms"": 105,
                ""contention_retries"": 0,
                ""storage_bytes_read"": 261,
                ""storage_bytes_write"": 0,
                ""rate_limits_hit"": []
            },
            ""schema_version"": 0
        }";
        var testMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseBody)
        };
        var conn = Mock.Create<IConnection>();
        Mock.Arrange(() =>
            conn.DoPostAsync(
                Arg.IsAny<string>(),
                Arg.IsAny<string>(),
                Arg.IsAny<Dictionary<string, string>>()
            )
        ).Returns(Task.FromResult(testMessage));

        var c = new Client(new ClientConfig("secret"), conn);
        var r = await c.QueryAsync<string>("let x = 123; x");

        bool check = false;

        Mock.Arrange(() =>
            conn.DoPostAsync(
                Arg.IsAny<string>(),
                Arg.IsAny<string>(),
                Arg.IsAny<Dictionary<string, string>>()
            )
        ).DoInstead((string path, string body, Dictionary<string, string> headers) => 
        {
            Assert.AreEqual(1702346199930000.ToString(), headers[Headers.LastTxnTs]);
            check = true;
        }).Returns(Task.FromResult(testMessage));

        var r2 = await c.QueryAsync<string>("let x = 123; x");

        Assert.IsTrue(check);
    }

    private static int GetIntFromReader(string input)
    {
        var reader = new Utf8FaunaReader(input);
        reader.Read();
        return reader.GetInt();
    }
}
