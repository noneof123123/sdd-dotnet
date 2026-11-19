using Microsoft.Extensions.Logging;
using RealtorApi.Infrastructure.Logging;

namespace RealtorApiTests.Infrastructure.Logging;

public sealed class InfrastructureLogTests
{
    [Fact]
    public void InfrastructureStarted_UsesStableEventAndNoSensitivePayload()
    {
        var logger = new RecordingLogger();

        InfrastructureLog.InfrastructureStarted(logger, "slice-discovery");

        Assert.Equal(1001, logger.EventId.Id);
        Assert.Equal(LogLevel.Information, logger.Level);
        Assert.DoesNotContain("password", logger.Message, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class RecordingLogger : ILogger
    {
        public LogLevel Level { get; private set; }
        public EventId EventId { get; private set; }
        public string Message { get; private set; } = string.Empty;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Level = logLevel;
            EventId = eventId;
            Message = formatter(state, exception);
        }
    }
}
