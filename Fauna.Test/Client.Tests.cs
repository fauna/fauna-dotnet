using Fauna.Constants;
using Fauna.Exceptions;
using Fauna.Serialization;
using NUnit.Framework;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using Telerik.JustMock;

namespace Fauna.Test;

[TestFixture]
public class ClientTests
{
    [AllowNull]
    private IConnection _mockConnection;
    [AllowNull]
    private ClientConfig _defaultConfig;


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

    private HttpResponseMessage MockQueryResponse(string responseBody, HttpStatusCode statusCode)
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
    [TestCase("unauthorized", 401, typeof(AuthenticationException), "Unauthorized: ")]
    [TestCase("forbidden", 403, typeof(AuthorizationException), "Forbidden: ")]
    [TestCase("invalid_query", 400, typeof(QueryCheckException), "Invalid Query: ")]
    [TestCase("invalid_function_definition", 400, typeof(QueryCheckException), "Invalid Query: ")]
    [TestCase("invalid_identifier", 400, typeof(QueryCheckException), "Invalid Query: ")]
    [TestCase("invalid_syntax", 400, typeof(QueryCheckException), "Invalid Query: ")]
    [TestCase("invalid_type", 400, typeof(QueryCheckException), "Invalid Query: ")]
    [TestCase("invalid_argument", 400, typeof(QueryRuntimeException), "Invalid Argument: ")]
    [TestCase("abort", 400, typeof(AbortException), "Abort: ")]
    [TestCase("invalid_request", 400, typeof(InvalidRequestException), "Invalid Request: ")]
    [TestCase("contended_transaction", 409, typeof(ContendedTransactionException), "Contended Transaction: ")]
    [TestCase("limit_exceeded", 429, typeof(ThrottlingException), "Limit Exceeded: ")]
    [TestCase("time_limit_exceeded", 440, typeof(QueryTimeoutException), "Time Limit Exceeded: ")]
    [TestCase("internal_error", 500, typeof(ServiceException), "Internal Error: ")]
    [TestCase("timeout", 503, typeof(QueryTimeoutException), "Timeout: ")]
    [TestCase("time_out", 503, typeof(QueryTimeoutException), "Timeout: ")]
    [TestCase("bad_gateway", 502, typeof(NetworkException), "Bad Gateway: ")]
    [TestCase("gateway_timeout", 504, typeof(NetworkException), "Gateway Timeout: ")]
    [TestCase("unexpected_error", 400, typeof(FaunaException), "Unexpected Error: ")] // Example for default case
    public void QueryAsync_ShouldThrowCorrectException_ForErrorCode(string errorCode, int httpStatus, Type expectedExceptionType, string expectedMessageStart)
    {
        var client = CreateClientWithMockConnection();

        HttpResponseMessage MockQR(string code, int status)
        {
            var body = $@"{{
                ""error"": {{
                    ""code"": ""{code}"",
                    ""message"": ""Mock message.""
                }},
                ""summary"": ""error: Mock message."",
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
            return MockQueryResponse(body, (HttpStatusCode)status);
        }

        Mock.Arrange(() => _mockConnection.DoPostAsync(
                Arg.IsAny<string>(),
                Arg.IsAny<Stream>(),
                Arg.IsAny<Dictionary<string, string>>()))
            .Returns(Task.FromResult(MockQR(errorCode, httpStatus)));

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
        var qr = MockQueryResponse(responseBody, HttpStatusCode.BadRequest);
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
        var qr = MockQueryResponse(responseBody, HttpStatusCode.OK);
        Mock.Arrange(() =>
                     _mockConnection.DoPostAsync(
                         Arg.IsAny<string>(),
                         Arg.IsAny<Stream>(),
                         Arg.IsAny<Dictionary<string, string>>())
        ).Returns(Task.FromResult(qr));

        var c = CreateClientWithMockConnection();
        var r = await c.QueryAsync<string>(new QueryExpr(new QueryLiteral("let x = 123; x")));

        bool check = false;

        var qr2 = MockQueryResponse(responseBody, HttpStatusCode.OK);
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
