using System;
using RentADad.Domain.Common;

namespace RentADad.Domain.Providers;

public sealed class ProviderAvailability
{
    private ProviderAvailability()
    {
    }

    public ProviderAvailability(Guid id, DateTime startUtc, DateTime endUtc)
    {
        if (id == Guid.Empty) throw new DomainRuleViolationException("Availability id is required.");
        if (endUtc <= startUtc) throw new DomainRuleViolationException("Availability end must be after start.");

        Id = id;
        StartUtc = startUtc;
        EndUtc = endUtc;
    }

    public Guid Id { get; }
    public DateTime StartUtc { get; }
    public DateTime EndUtc { get; }

    public bool Overlaps(DateTime startUtc, DateTime endUtc)
    {
        return startUtc < EndUtc && endUtc > StartUtc;
    }

    public bool Contains(DateTime startUtc, DateTime endUtc)
    {
        return startUtc >= StartUtc && endUtc <= EndUtc;
    }
}
