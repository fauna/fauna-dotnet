using Fauna.Constants;
using Fauna.Exceptions;
using Fauna.Serialization;
using Fauna.Test.Helpers;
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

    private async Task<HttpResponseMessage> MockQueryResponseAsync(string responseBody, HttpStatusCode statusCode)
    {
        var testMessage = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(responseBody)
        };
        return testMessage;
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
            var r = await c.QueryAsync<int>(
                new QueryExpr(new QueryLiteral($"let x = {expected}; abort(x)")),
                new QueryOptions { QueryTags = new Dictionary<string, string> { { "foo", "bar" }, { "baz", "luhrmann" } } });
        }
        catch (AbortException ex)
        {
            var abortData = ex.GetData();
            Assert.AreEqual("abort", ex.QueryFailure?.ErrorInfo.Code);
            Assert.IsInstanceOf<int>(abortData);
            Assert.AreEqual(expected, abortData);
            Console.WriteLine(ex.QueryFailure?.Summary);
        }
    }

    [Test]
    [TestCase("unauthorized", typeof(AuthenticationException), "Unauthorized: ")]
    [TestCase("forbidden", typeof(AuthorizationException), "Forbidden: ")]
    [TestCase("invalid_query", typeof(QueryCheckException), "Invalid Query: ")]
    [TestCase("invalid_function_definition", typeof(QueryCheckException), "Invalid Query: ")]
    [TestCase("invalid_identifier", typeof(QueryCheckException), "Invalid Query: ")]
    [TestCase("invalid_syntax", typeof(QueryCheckException), "Invalid Query: ")]
    [TestCase("invalid_type", typeof(QueryCheckException), "Invalid Query: ")]
    [TestCase("invalid_argument", typeof(QueryRuntimeException), "Invalid Argument: ")]
    [TestCase("abort", typeof(AbortException), "Abort: ")]
    [TestCase("invalid_request", typeof(InvalidRequestException), "Invalid Request: ")]
    [TestCase("contended_transaction", typeof(ContendedTransactionException), "Contended Transaction: ")]
    [TestCase("limit_exceeded", typeof(ThrottlingException), "Limit Exceeded: ")]
    [TestCase("time_limit_exceeded", typeof(QueryTimeoutException), "Time Limit Exceeded: ")]
    [TestCase("internal_error", typeof(ServiceException), "Internal Error: ")]
    [TestCase("timeout", typeof(QueryTimeoutException), "Timeout: ")]
    [TestCase("time_out", typeof(QueryTimeoutException), "Timeout: ")]
    [TestCase("bad_gateway", typeof(NetworkException), "Bad Gateway: ")]
    [TestCase("gateway_timeout", typeof(NetworkException), "Gateway Timeout: ")]
    [TestCase("unexpected_error", typeof(FaunaException), "Unexpected Error: ")] // Example for default case
    public async Task QueryAsync_ShouldThrowCorrectException_ForErrorCode(string errorCode, Type expectedExceptionType, string expectedMessageStart)
    {
        var client = CreateClientWithMockConnection();

        Mock.Arrange(() => _mockConnection.DoPostAsync<object>(
                Arg.IsAny<string>(),
                Arg.IsAny<Stream>(),
                Arg.IsAny<Dictionary<string, string>>()))
            .Returns(Task.FromResult<QueryResponse>(ExceptionTestHelper.CreateQueryFailure(errorCode)));

        async Task TestDelegate() => await client.QueryAsync<object>(new QueryExpr(new QueryLiteral("let x = 123; x")));

        var exception = Assert.ThrowsAsync(expectedExceptionType, TestDelegate);
        Assert.That(exception?.Message, Does.StartWith(expectedMessageStart));
    }

    [Test]
    public async Task QueryAsync_ShouldThrowAbortExceptionWithCorrectObject_OnAbortErrorCode()
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
        var qr = await MockQueryResponseAsync(responseBody, HttpStatusCode.BadRequest);
        Mock.Arrange(() => _mockConnection.DoPostAsync(Arg.IsAny<string>(), Arg.IsAny<Stream>(), Arg.IsAny<Dictionary<string, string>>())).Returns(Task.FromResult(qr));
        var c = CreateClientWithMockConnection();

        try
        {
            var query = new QueryExpr(new QueryLiteral($"let x = {expected}; abort(x)"));
            var r = await c.QueryAsync<string>(query);
        }
        catch (AbortException ex)
        {
            var abortData = ex.GetData();
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
        var qr = await MockQueryResponseAsync(responseBody, HttpStatusCode.OK);
        Mock.Arrange(() =>
                     _mockConnection.DoPostAsync(
                         Arg.IsAny<string>(),
                         Arg.IsAny<Stream>(),
                         Arg.IsAny<Dictionary<string, string>>())
        ).Returns(Task.FromResult(qr));

        var c = CreateClientWithMockConnection();
        var r = await c.QueryAsync<string>(new QueryExpr(new QueryLiteral("let x = 123; x")));

        bool check = false;

        var qr2 = await MockQueryResponseAsync(responseBody, HttpStatusCode.OK);
        Mock.Arrange(() =>
            _mockConnection.DoPostAsync(
                Arg.IsAny<string>(),
                Arg.IsAny<Stream>(),
                Arg.IsAny<Dictionary<string, string>>()
            )
        ).DoInstead((string path, Stream body, Dictionary<string, string> headers) =>
        {
            Assert.AreEqual(1702346199930000.ToString(), headers[Headers.LastTxnTs]);
            check = true;
        }).Returns(Task.FromResult(qr2));

        var r2 = await c.QueryAsync<string>(new QueryExpr(new QueryLiteral("let x = 123; x")));

        Assert.IsTrue(check);
    }
}
