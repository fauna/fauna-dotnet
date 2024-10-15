using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using static Fauna.Query;

namespace Fauna.Test.Util;

[TestFixture]
public class LoggingTests
{
    [Test]
    public async Task ValidateCustomLogger()
    {
        var logger = new Mock<ILogger>();
        logger
            .Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ))
            .Verifiable();

        var client = new Client(
            new Configuration("secret", logger: logger.Object)
            {
                Endpoint = new Uri("http://localhost:8443")
            }
        );

        await client.QueryAsync(FQL($"1+1"));

        logger.Verify(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.AtLeastOnce);
    }
}
