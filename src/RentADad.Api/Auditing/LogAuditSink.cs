using RentADad.Application.Abstractions.Auditing;

namespace RentADad.Api.Auditing;

public sealed class LogAuditSink : IAuditSink
{
    private readonly ILogger<LogAuditSink> _logger;

    public LogAuditSink(ILogger<LogAuditSink> logger)
    {
        _logger = logger;
    }

    public Task WriteAsync(string eventName, object payload, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Audit {EventName} {Payload}", eventName, payload);
        return Task.CompletedTask;
    }
}
