using Fauna.Constants;
using Fauna.Serialization;
using NUnit.Framework;
using System.Buffers;
using System.Net;
using System.Text;
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
    [Ignore("connected test")]
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
            new QueryExpr(new QueryLiteral("let x = 123; x")),
            new QueryOptions { QueryTags = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "luhrmann" } } });
        Write(r.Data);
        Console.WriteLine(string.Join(',', r.QueryTags!.Select(kv => $"{kv.Key}={kv.Value}")));
    }

    [Test]
    [Ignore("connected test")]
    public async Task CreateClientError()
    {
        var expected = 123;
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
                new QueryExpr(new QueryLiteral($"let x = {expected}; abort(x)")),
                new QueryOptions { QueryTags = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "luhrmann" } } });
        }
        catch (FaunaAbortException ex)
        {
            Assert.AreEqual("abort", ex.QueryFailure.ErrorInfo.Code);
            Assert.IsInstanceOf<int>(ex.AbortData);
            Assert.AreEqual(expected, ex.AbortData);
            Console.WriteLine(ex.QueryFailure.Summary);
        }
    }

    [Test]
    public async Task AbortReturnsQueryFailureAndThrows()
    {
        var expected = 123;
        var responseBody = $@"{{
            ""error"": {{
                ""code"": ""abort"",
                ""message"": ""Query aborted."",
                ""abort"": ""{{\""@int\"":\""{expected}\""}}""
            }},
            ""summary"": ""error: Query aborted.\nat *query*:1:19\n  |\n1 | let x = {expected}; abort(x)\n  |                   ^^^\n  |"",
            ""txn_ts"": 1702346199930000,
            ""stats"": {{
                ""compute_ops"": 1,
                ""read_ops"": 0,
                ""write_ops"": 0,
                ""query_time_ms"": 105,
                ""contention_retries"": 0,
                ""storage_bytes_read"": 261,
                ""storage_bytes_write"": 0,
                ""rate_limits_hit"": []
            }},
            ""schema_version"": 0
        }}";
        var testMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(responseBody)
        };
        var qr = await QueryResponse.GetFromHttpResponseAsync<string>(testMessage);
        var conn = Mock.Create<IConnection>();
        Mock.Arrange(() =>
            conn.DoPostAsync<string>(
                Arg.IsAny<string>(),
                Arg.IsAny<Stream>(),
                Arg.IsAny<Dictionary<string, string>>()
            )
        ).Returns(Task.FromResult(qr));

        var c = new Client(new ClientConfig("secret"), conn);

        try
        {
            var query = new QueryExpr(new QueryLiteral($"let x = {expected}; abort(x)"));
            var r = await c.QueryAsync<string>(query);
        }
        catch (FaunaAbortException ex)
        {
            Assert.AreEqual("abort", ex.QueryFailure.ErrorInfo.Code);
            Assert.IsInstanceOf<int>(ex.AbortData);
            Assert.AreEqual(expected, ex.AbortData);
        }
    }

    [Test]
    public async Task LastSeenTxnPropagatesToSubsequentQueries()
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
        var qr = await QueryResponse.GetFromHttpResponseAsync<string>(testMessage);
        var conn = Mock.Create<IConnection>();
        Mock.Arrange(() =>
            conn.DoPostAsync<string>(
                Arg.IsAny<string>(),
                Arg.IsAny<Stream>(),
                Arg.IsAny<Dictionary<string, string>>()
            )
        ).Returns(Task.FromResult(qr));

        var c = new Client(new ClientConfig("secret"), conn);
        var r = await c.QueryAsync<string>(new QueryExpr(new QueryLiteral("let x = 123; x")));

        bool check = false;

        Mock.Arrange(() =>
            conn.DoPostAsync<string>(
                Arg.IsAny<string>(),
                Arg.IsAny<Stream>(),
                Arg.IsAny<Dictionary<string, string>>()
            )
        ).DoInstead((string path, Stream body, Dictionary<string, string> headers) =>
        {
            Assert.AreEqual(1702346199930000.ToString(), headers[Headers.LastTxnTs]);
            check = true;
        }).Returns(Task.FromResult(qr));

        var r2 = await c.QueryAsync<string>(new QueryExpr(new QueryLiteral("let x = 123; x")));

        Assert.IsTrue(check);
    }
}
