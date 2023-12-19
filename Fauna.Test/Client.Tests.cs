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
    private IConnection? _mockConnection;
    private ClientConfig? _defaultConfig;


    [SetUp]
    public void SetUp()
    {
        _mockConnection = Mock.Create<IConnection>();
        _defaultConfig = new ClientConfig("secret")
        {
            Endpoint = Endpoints.Local,
            DefaultQueryOptions = new QueryOptions
            {
                QueryTags = new Dictionary<string, string> { { "lorem", "ipsum" } }
            },
            ConnectionTimeout = TimeSpan.FromSeconds(10)
        };
    }

    private Client CreateClientWithMockConnection(ClientConfig? config = null, IConnection? connection = null)
    {
        return new Client(
            config ?? _defaultConfig ?? throw new InvalidOperationException("Default config is not set"),
            connection ?? _mockConnection ?? throw new InvalidOperationException("Mock connection is not set")
        );
    }

    private Client CreateClient(ClientConfig? config = null, IConnection? connection = null)
    {
        return connection != null
            ? new Client(config ?? _defaultConfig ?? throw new InvalidOperationException("Default config is not set"), connection)
            : new Client(config ?? _defaultConfig ?? throw new InvalidOperationException("Default config is not set"));
    }

    private async Task<QueryResponse> MockQueryResponseAsync<T>(string responseBody, HttpStatusCode statusCode)
    {
        var testMessage = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(responseBody)
        };
        return await QueryResponse.GetFromHttpResponseAsync<T>(testMessage);
    }

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
    public async Task CreateClientTest()
    {
        var c = CreateClient();
        var r = await c.QueryAsync<int>(
            new QueryExpr(new QueryLiteral("let x = 123; x")),
            new QueryOptions { QueryTags = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "luhrmann" } } });
        Write(r.Data.ToString());
        Console.WriteLine(string.Join(',', r.QueryTags!.Select(kv => $"{kv.Key}={kv.Value}")));
    }

    [Test]
    [Ignore("connected test")]
    public async Task CreateClientError()
    {
        var expected = 123;
        var c = CreateClient();

        try
        {
            var r = await c.QueryAsync<string>(
                new QueryExpr(new QueryLiteral($"let x = {expected}; abort(x)")),
                new QueryOptions { QueryTags = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "luhrmann" } } });
        }
        catch (FaunaAbortException ex)
        {
            var abortData = ex.GetData<int>();
            Assert.AreEqual("abort", ex.QueryFailure?.ErrorInfo.Code);
            Assert.IsInstanceOf<int>(abortData);
            Assert.AreEqual(expected, abortData);
            Console.WriteLine(ex.QueryFailure?.Summary);
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
        var qr = await MockQueryResponseAsync<string>(responseBody, HttpStatusCode.BadRequest);
        Mock.Arrange(() => _mockConnection.DoPostAsync<string>(Arg.IsAny<string>(), Arg.IsAny<Stream>(), Arg.IsAny<Dictionary<string, string>>())).Returns(Task.FromResult(qr));
        var c = CreateClientWithMockConnection();

        try
        {
            var query = new QueryExpr(new QueryLiteral($"let x = {expected}; abort(x)"));
            var r = await c.QueryAsync<string>(query);
        }
        catch (FaunaAbortException ex)
        {
            var abortData = ex.GetData<int>();
            Assert.AreEqual("abort", ex.QueryFailure?.ErrorInfo.Code);
            Assert.IsInstanceOf<int>(abortData);
            Assert.AreEqual(expected, abortData);
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
        var qr = await MockQueryResponseAsync<string>(responseBody, HttpStatusCode.OK);
        Mock.Arrange(() => _mockConnection.DoPostAsync<string>(Arg.IsAny<string>(), Arg.IsAny<Stream>(), Arg.IsAny<Dictionary<string, string>>())).Returns(Task.FromResult(qr));

        var c = CreateClientWithMockConnection();
        var r = await c.QueryAsync<string>(new QueryExpr(new QueryLiteral("let x = 123; x")));

        bool check = false;

        Mock.Arrange(() =>
            _mockConnection.DoPostAsync<string>(
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
