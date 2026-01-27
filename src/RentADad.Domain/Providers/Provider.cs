using System;
using System.Collections.Generic;
using System.Linq;
using RentADad.Domain.Common;

namespace RentADad.Domain.Providers;

public sealed class Provider
{
    private readonly List<ProviderAvailability> _availabilities = new();

    private Provider()
    {
        DisplayName = string.Empty;
        UpdatedUtc = DateTime.UtcNow;
    }

    public Provider(Guid id, string displayName)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("Provider id is required.");

        Id = id;
        DisplayName = displayName ?? string.Empty;
        UpdatedUtc = DateTime.UtcNow;
    }

    public Guid Id { get; }
    public string DisplayName { get; private set; }
    public IReadOnlyCollection<ProviderAvailability> Availabilities => _availabilities;
    public DateTime UpdatedUtc { get; private set; }

    public void UpdateDisplayName(string displayName)
    {
        DisplayName = displayName ?? string.Empty;
    }

    public ProviderAvailability AddAvailability(DateTime startUtc, DateTime endUtc)
    {
        if (endUtc <= startUtc) throw new DomainRuleViolationException("Availability end must be after start.");
        if (_availabilities.Any(a => a.Overlaps(startUtc, endUtc)))
            throw new DomainRuleViolationException("Availability windows must not overlap.");

        var availability = new ProviderAvailability(Guid.NewGuid(), startUtc, endUtc);
        _availabilities.Add(availability);
        return availability;
    }

    public void RemoveAvailability(Guid availabilityId)
    {
        var availability = _availabilities.FirstOrDefault(a => a.Id == availabilityId);
        if (availability == null) return;
        _availabilities.Remove(availability);
    }

    public bool IsAvailable(DateTime startUtc, DateTime endUtc)
    {
        return _availabilities.Any(a => a.Contains(startUtc, endUtc));
    }
}
