using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RentADad.Application.Abstractions.Repositories;
using RentADad.Application.Bookings;

namespace RentADad.Api.Background;

public sealed class BookingExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingExpiryService> _logger;
    private readonly bool _enabled;
    private readonly int _intervalSeconds;
    private readonly int _batchSize;

    public BookingExpiryService(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<BookingExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _enabled = configuration.GetValue("BookingExpiry:Enabled", true);
        _intervalSeconds = configuration.GetValue("BookingExpiry:IntervalSeconds", 60);
        _batchSize = configuration.GetValue("BookingExpiry:BatchSize", 100);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_enabled)
        {
            _logger.LogInformation("Booking expiry background service disabled.");
            return;
        }

        _logger.LogInformation("Booking expiry background service started.");

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_intervalSeconds));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await ExpireBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Booking expiry background service failed.");
            }
        }
    }

    private async Task ExpireBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var service = scope.ServiceProvider.GetRequiredService<BookingService>();

        var now = DateTime.UtcNow;
        var expiredIds = await repository.ListExpiredPendingAsync(now, _batchSize, cancellationToken);
        if (expiredIds.Count == 0)
        {
            return;
        }

        foreach (var bookingId in expiredIds)
        {
            await service.ExpireAsync(bookingId, cancellationToken);
        }

        _logger.LogInformation("Expired {Count} pending bookings.", expiredIds.Count);
    }
}
