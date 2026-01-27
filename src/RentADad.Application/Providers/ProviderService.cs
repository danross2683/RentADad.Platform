using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RentADad.Application.Abstractions.Persistence;
using RentADad.Application.Abstractions.Repositories;
using RentADad.Application.Providers.Requests;
using RentADad.Application.Providers.Responses;
using RentADad.Domain.Common;
using RentADad.Domain.Providers;

namespace RentADad.Application.Providers;

public sealed class ProviderService
{
    private readonly IProviderRepository _providers;
    private readonly IUnitOfWork _unitOfWork;

    public ProviderService(IProviderRepository providers, IUnitOfWork unitOfWork)
    {
        _providers = providers;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProviderResponse?> GetAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        var provider = await _providers.GetByIdAsync(providerId, cancellationToken);
        return provider is null ? null : ToResponse(provider);
    }

    public async Task<ProviderResponse> RegisterAsync(RegisterProviderRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var providerId = request.ProviderId ?? Guid.NewGuid();
            var provider = new Provider(providerId, request.DisplayName ?? string.Empty);
            _providers.Add(provider);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);
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
            await _unitOfWork.SaveChangesAsync(cancellationToken);
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(provider);
    }

    private static ProviderResponse ToResponse(Provider provider)
    {
        var availabilities = provider.Availabilities
            .Select(a => new ProviderAvailabilityResponse(a.Id, a.StartUtc, a.EndUtc))
            .ToList();

        return new ProviderResponse(provider.Id, provider.DisplayName, availabilities);
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
