using System.Diagnostics.CodeAnalysis;
using System.Net;
using Fauna.Exceptions;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Fauna.Test;

[TestFixture]
public class ConnectionTests
{
    private const string TestBaseUri = "http://testing/";
    private const string TestQueryUri = $"{TestBaseUri}query/1";
    private const int MaxRetries = 3;

    [AllowNull] private Client _client;

    [AllowNull] private Mock<HttpMessageHandler> _handlerMock;

    private Func<HttpStatusCode> _simulateRetries = () => HttpStatusCode.OK;

    private readonly Query _fql = Query.FQL($"1+1");

    [SetUp]
    public void OneTimeSetup()
    {
        _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync((HttpRequestMessage _, CancellationToken _) => new HttpResponseMessage()
            {
                StatusCode = _simulateRetries(),
                Content = new StringContent(
                    "{\"data\": {\"@int\": \"2\"}, \"summary\": \"\", \"txn_ts\": 1722970377042000, \"stats\": {\"compute_ops\": 1, \"read_ops\": 0, \"write_ops\": 0, \"query_time_ms\": 325, \"contention_retries\": 0, \"storage_bytes_read\": 0, \"storage_bytes_write\": 0, \"rate_limits_hit\": []}, \"schema_version\": 172295678446000}")
            })
            .Verifiable();

        var httpClient = new HttpClient(_handlerMock.Object) { BaseAddress = new Uri(TestBaseUri), };

        Configuration configuration = new("", httpClient)
        {
            Endpoint = new Uri(TestBaseUri),
            RetryConfiguration = new RetryConfiguration(MaxRetries, TimeSpan.FromMilliseconds(20))
        };
        _client = new Client(configuration);
    }

    [Test]
    public async Task TestMockQuery()
    {
        // ACT
        await _client.QueryAsync<int>(_fql);

        // ASSERT
        _handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post
                && req.RequestUri!.ToString() == TestQueryUri
            ),
            ItExpr.IsAny<CancellationToken>()
        );
    }


    [Test]
    [Repeat(MaxRetries + 1)]
    public async Task TestMockQueryWithRetries()
    {
        // ARRANGE
        int repeatIndex = TestContext.CurrentContext.CurrentRepeatCount;
        int retryCount = 0;

        _simulateRetries = () =>
        {
            HttpStatusCode responseCode = HttpStatusCode.TooManyRequests;
            if (repeatIndex < retryCount)
            {
                responseCode = HttpStatusCode.OK;
            }

            Interlocked.Increment(ref retryCount);
            return responseCode;
        };


        if (repeatIndex < MaxRetries)
        {
            // ACT
            await _client.QueryAsync<int>(_fql);

            // ASSERT
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(retryCount),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post
                    && req.RequestUri!.ToString() == TestQueryUri
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
        else
        {
            Assert.ThrowsAsync<MaxRetriesException>(async () => await _client.QueryAsync<int>(_fql));
        }
    }
}
