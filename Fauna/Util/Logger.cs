using Microsoft.Extensions.Logging;

namespace Fauna.Util;

/// <summary>
/// This class encapsulates an <see cref="ILogger"/> object for logging throughout
/// the Fauna .NET driver business logic.
/// </summary>
internal static class Logger
{
    private static ILogger? s_logger;

    /// <summary>
    /// The singleton <see cref="ILogger"/> instance to use for logging
    /// </summary>
    public static ILogger Instance
    {
        get
        {
            if (s_logger == null)
            {
                s_logger = InitializeDefaultLogger();
            }

            return s_logger;
        }
    }

    /// <summary>
    /// Optionally initialize the internal <see cref="ILogger"/> with a custom one
    /// </summary>
    /// <param name="logger">The <see cref="ILogger"/> instance to use for logging</param>
    public static void Initialize(ILogger logger)
    {
        s_logger = logger;
    }

    /// <summary>
    /// Initializes a default Console logger with single-line output, UTC timestamps, and
    /// minimum log-level based on the FAUNA_DEBUG environment variable; if the variable is
    /// not set, the default minimum log-level is <see cref="LogLevel.None"/>
    /// </summary>
    /// <returns>An instance of <see cref="ILogger"/> created with <see cref="ILoggerFactory"/></returns>
    /// <exception cref="ArgumentException">Throws if FAUNA_DEBUG is outside of acceptable values</exception>
    private static ILogger InitializeDefaultLogger()
    {
        var logLevel = Environment.GetEnvironmentVariable("FAUNA_DEBUG");
        var minLogLevel = LogLevel.None;

        if (!string.IsNullOrEmpty(logLevel) && int.TryParse(logLevel, out var level))
        {
            if (level < (int)LogLevel.Trace || level > (int)LogLevel.None)
            {
                throw new ArgumentException(
                    $"Invalid FAUNA_DEBUG value of {level}; must be between 0 and 6 inclusive. Set to 0 for highest verbosity, default is 6 (no logging).");
            }

            minLogLevel = (LogLevel)level;
        }

        using ILoggerFactory factory = LoggerFactory.Create(builder => builder
            .AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace)
            .AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";
                options.UseUtcTimestamp = true;
            })
            .SetMinimumLevel(minLogLevel));

        return factory.CreateLogger("fauna-dotnet");
    }
}
