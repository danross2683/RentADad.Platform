using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RentADad.Application.Abstractions.Persistence;
using RentADad.Application.Abstractions.Repositories;
using RentADad.Application.Abstractions.Notifications;
using RentADad.Application.Abstractions.Auditing;
using RentADad.Application.Abstractions.Caching;
using RentADad.Application.Common.Paging;
using RentADad.Application.Providers.Requests;
using RentADad.Application.Providers.Responses;
using RentADad.Domain.Common;
using RentADad.Domain.Providers;

namespace RentADad.Application.Providers;

public sealed class ProviderService
{
    private readonly IProviderRepository _providers;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationSender _notifications;
    private readonly IAuditSink _auditSink;
    private readonly ICacheStore _cache;
    private readonly CacheSettings _cacheSettings;
    private readonly ILogger<ProviderService> _logger;

    public ProviderService(
        IProviderRepository providers,
        IUnitOfWork unitOfWork,
        INotificationSender notifications,
        IAuditSink auditSink,
        ICacheStore cache,
        CacheSettings cacheSettings,
        ILogger<ProviderService> logger)
    {
        _providers = providers;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
        _auditSink = auditSink;
        _cache = cache;
        _cacheSettings = cacheSettings;
        _logger = logger;
    }

    public async Task<ProviderResponse?> GetAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetProviderCacheKey(providerId);
        if (_cache.TryGet(cacheKey, out ProviderResponse? cached))
        {
            return cached;
        }

        var provider = await _providers.GetByIdAsync(providerId, cancellationToken);
        if (provider is null) return null;

        var response = ToResponse(provider);
        _cache.Set(cacheKey, response, TimeSpan.FromSeconds(_cacheSettings.ProviderAvailabilitySeconds));
        return response;
    }

    public async Task<PagedResult<ProviderResponse>> ListAsync(ProviderListQuery query, CancellationToken cancellationToken = default)
    {
        var paged = await _providers.ListAsync(query, cancellationToken);
        var items = paged.Items.Select(ToResponse).ToList();
        return new PagedResult<ProviderResponse>(items, paged.Page, paged.PageSize, paged.TotalCount);
    }

    public async Task<ProviderResponse> RegisterAsync(RegisterProviderRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var providerId = request.ProviderId ?? Guid.NewGuid();
            var provider = new Provider(providerId, request.DisplayName ?? string.Empty);
            _providers.Add(provider);
            _logger.LogInformation("Provider registered {ProviderId}", provider.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _notifications.NotifyAsync("provider.registered", new { provider.Id, provider.DisplayName }, cancellationToken);
            await _auditSink.WriteAsync("provider.registered", new { provider.Id, provider.DisplayName }, cancellationToken);
            _cache.Remove(GetProviderCacheKey(provider.Id));
            return ToResponse(provider);
        }
        catch (DomainRuleViolationException ex)
        {
            throw new ProviderDomainException(ex.Message, MapProviderErrorCode(ex.Message));
        }
    }

    public async Task<ProviderResponse?> UpdateAsync(Guid providerId, UpdateProviderRequest request, CancellationToken cancellationToken = default)
    {
        var provider = await _providers.GetForUpdateAsync(providerId, cancellationToken);
        if (provider is null) return null;

        provider.UpdateDisplayName(request.DisplayName ?? string.Empty);
        _logger.LogInformation("Provider updated {ProviderId}", provider.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _notifications.NotifyAsync("provider.updated", new { provider.Id }, cancellationToken);
        await _auditSink.WriteAsync("provider.updated", new { provider.Id }, cancellationToken);
        _cache.Remove(GetProviderCacheKey(provider.Id));
        return ToResponse(provider);
    }

    public async Task<ProviderResponse?> AddAvailabilityAsync(Guid providerId, AddAvailabilityRequest request, CancellationToken cancellationToken = default)
    {
        var provider = await _providers.GetForUpdateAsync(providerId, cancellationToken);
        if (provider is null) return null;

        try
        {
            ValidateTimeWindow(request.StartUtc, request.EndUtc);
            provider.AddAvailability(request.StartUtc, request.EndUtc);
            _logger.LogInformation("Provider availability added {ProviderId}", provider.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _notifications.NotifyAsync("provider.availability_added", new { provider.Id }, cancellationToken);
            await _auditSink.WriteAsync("provider.availability_added", new { provider.Id }, cancellationToken);
            _cache.Remove(GetProviderCacheKey(provider.Id));
            return ToResponse(provider);
        }
        catch (ProviderDomainException)
        {
            throw;
        }
        catch (DomainRuleViolationException ex)
        {
            throw new ProviderDomainException(ex.Message, MapProviderErrorCode(ex.Message));
        }
    }

    public async Task<ProviderResponse?> RemoveAvailabilityAsync(Guid providerId, Guid availabilityId, CancellationToken cancellationToken = default)
    {
        var provider = await _providers.GetForUpdateAsync(providerId, cancellationToken);
        if (provider is null) return null;

        provider.RemoveAvailability(availabilityId);
        _logger.LogInformation("Provider availability removed {ProviderId} {AvailabilityId}", provider.Id, availabilityId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _notifications.NotifyAsync("provider.availability_removed", new { provider.Id, availabilityId }, cancellationToken);
        await _auditSink.WriteAsync("provider.availability_removed", new { provider.Id, availabilityId }, cancellationToken);
        _cache.Remove(GetProviderCacheKey(provider.Id));
        return ToResponse(provider);
    }

    public async Task<ProviderResponse?> ReplaceAvailabilityAsync(Guid providerId, ReplaceAvailabilityRequest request, CancellationToken cancellationToken = default)
    {
        var provider = await _providers.GetForUpdateAsync(providerId, cancellationToken);
        if (provider is null) return null;

        try
        {
            provider.ClearAvailabilities();
            foreach (var slot in request.Slots)
            {
                ValidateTimeWindow(slot.StartUtc, slot.EndUtc);
                provider.AddAvailability(slot.StartUtc, slot.EndUtc);
            }

            _logger.LogInformation("Provider availability replaced {ProviderId}", provider.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _notifications.NotifyAsync("provider.availability_replaced", new { provider.Id }, cancellationToken);
            await _auditSink.WriteAsync("provider.availability_replaced", new { provider.Id }, cancellationToken);
            _cache.Remove(GetProviderCacheKey(provider.Id));
            return ToResponse(provider);
        }
        catch (ProviderDomainException)
        {
            throw;
        }
        catch (DomainRuleViolationException ex)
        {
            throw new ProviderDomainException(ex.Message, MapProviderErrorCode(ex.Message));
        }
    }

    private static string GetProviderCacheKey(Guid providerId) => $"provider:{providerId}";

    private static ProviderResponse ToResponse(Provider provider)
    {
        var availabilities = provider.Availabilities
            .Select(a => new ProviderAvailabilityResponse(a.Id, a.StartUtc, a.EndUtc))
            .ToList();

        return new ProviderResponse(provider.Id, provider.DisplayName, availabilities, provider.UpdatedUtc);
    }

    private static string MapProviderErrorCode(string message)
    {
        if (message.Contains("Availability windows must not overlap", StringComparison.OrdinalIgnoreCase))
            return "provider_availability_overlap";
        if (message.Contains("Availability end must be after start", StringComparison.OrdinalIgnoreCase))
            return "provider_availability_invalid_range";
        if (message.Contains("Provider id is required", StringComparison.OrdinalIgnoreCase))
            return "provider_id_required";
        if (message.Contains("Availability id is required", StringComparison.OrdinalIgnoreCase))
            return "provider_availability_id_required";

        return "provider_rule_violation";
    }

    private static void ValidateTimeWindow(DateTime startUtc, DateTime endUtc)
    {
        if (startUtc.Kind != DateTimeKind.Utc || endUtc.Kind != DateTimeKind.Utc)
            throw new ProviderDomainException("StartUtc and EndUtc must be UTC.", "provider_availability_time_not_utc");
        if (startUtc < DateTime.UtcNow)
            throw new ProviderDomainException("StartUtc must be in the future.", "provider_availability_time_in_past");
        if (endUtc <= startUtc)
            throw new ProviderDomainException("EndUtc must be after StartUtc.", "provider_availability_invalid_range");
    }
}
