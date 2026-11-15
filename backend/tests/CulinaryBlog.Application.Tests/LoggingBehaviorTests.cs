using CulinaryBlog.Application.Common.Behaviors;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using MediatR;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class LoggingBehaviorTests
{
    [Fact]
    public async Task Handle_LogsSlowRequestMetadataWithoutRequestPayload()
    {
        var logger = new CapturingLogger<LoggingBehavior<SecretRequest, string>>();
        var userId = Guid.NewGuid();
        var behavior = new LoggingBehavior<SecretRequest, string>(
            logger,
            new TestCurrentUser(userId));
        const string secret = "do-not-log-this";

        var response = await behavior.Handle(
            new SecretRequest(secret),
            async cancellationToken =>
            {
                await Task.Delay(510, cancellationToken);
                return "completed";
            },
            CancellationToken.None);

        var entry = Assert.Single(logger.Entries);
        Assert.Equal("completed", response);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Equal(nameof(SecretRequest), entry.Properties["RequestType"]);
        Assert.Equal(userId, entry.Properties["UserId"]);
        Assert.True((long)entry.Properties["ElapsedMilliseconds"]! > 500);
        Assert.DoesNotContain(secret, entry.Message);
        Assert.DoesNotContain("Password", entry.Properties.Keys);
    }

    private sealed record SecretRequest(string Password);

    private sealed class TestCurrentUser(Guid userId) : ICurrentUser
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => true;
        public bool IsAuthor => false;
        public bool IsAdmin => false;
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var properties = state is IEnumerable<KeyValuePair<string, object?>> values
                ? values.ToDictionary(value => value.Key, value => value.Value)
                : new Dictionary<string, object?>();
            Entries.Add(new LogEntry(logLevel, formatter(state, exception), properties));
        }
    }

    private sealed record LogEntry(
        LogLevel Level,
        string Message,
        IReadOnlyDictionary<string, object?> Properties);
}
